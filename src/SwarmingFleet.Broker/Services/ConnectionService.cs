
namespace SwarmingFleet.Broker.Services
{
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using SwarmingFleet.Broker.DAL;
    using SwarmingFleet.Collections.Generic;
    using SwarmingFleet.Contracts;
    using SwarmingFleet.DAL;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using static SwarmingFleet.Contracts.ConnectionService;

    public class ConnectionService : ConnectionServiceBase
    {
        private readonly IMemoryCache _nonceCache;
        private readonly IMemoryCache _tokenCache;
        private readonly BrokerContext _context;

        public ConnectionService(IMemoryCache nonceCache, IMemoryCache tokenCache, BrokerContext context)
        {
            this._nonceCache = nonceCache;
            this._tokenCache = tokenCache;
            this._context = context;
        }

        public override Task<NonceReply> Prelogin(NonceRequest request, ServerCallContext context)
        { 
            if(!this._nonceCache.TryGetValue(context.Peer, out NonceReply nonceReply))
            {
                // todo: 需關切此勞動者端點呼叫頻率並記錄狀況，在必要時控制防火牆行為


                var nonce = BrokerSideUtils.GetNonce();
                var expiredTime = DateTime.UtcNow.AddSeconds(60);
                nonceReply = new NonceReply
                {
                    ExpiredTime = Timestamp.FromDateTime(expiredTime), 
                    Nonce = nonce
                };
                this._nonceCache.Set(context.Peer, nonceReply, expiredTime);
            }

            return Task.FromResult(nonceReply);
        }

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                // 如果該勞動者端點所對應的權杖能找到，表示重複登入
                if (this._tokenCache.TryGetValue(context.Peer, out Token token))
                {
                    // todo: 需關切此勞動者端點登入情形，並記錄登入狀況，在必要時控制防火牆行為
                    return new LoginReply { Error = LoginErrors.RepeatedLogon };
                }

                // 如果能找到隨機碼，表示隨機碼還沒過期
                if (this._nonceCache.TryGetValue(context.Peer, out NonceReply nonceReply))
                {
                    // 移除快取中的隨機碼
                    this._nonceCache.Remove(context.Peer);
                }
                // 未經驗證，需要重新呼叫 Prelogin 
                else
                {
                    // todo: 需關切此勞動者端點登入情形，並記錄登入狀況，在必要時控制防火牆行為
                    return  new LoginReply { Error = LoginErrors.Unauthorized } ;
                }

                var now = DateTime.UtcNow;
                var dhk = request.Signature.ToBase64();

                // 如果能找到對應的裝置硬體金鑰
                if (this._context.KeyPairs.FirstOrDefault(x => x.Dhk.Equals(dhk)) is KeyPair keyPair &&
                    request.Hash.Equals(BrokerSideUtils.ComputeHash(nonceReply.Nonce, request.Nonce, keyPair.Spk)) &&
                    keyPair.Registered)
                {
                    Console.WriteLine("n+");
                    keyPair.LastOnlineTime = now;
                    this._context.Update(keyPair);
                    this._context.SaveChanges();
                    goto Logon;
                }
                // 如果能透過伺服器預產金鑰及其雜湊找到，則為
                else
                { 
                    foreach (var kp in this._context.KeyPairs)
                    {
                        var hash = BrokerSideUtils.ComputeHash(nonceReply.Nonce, request.Nonce, kp.Spk);
                        // 第一次登入，需綁定資料
                        if (hash.Equals(request.Hash) && !kp.Registered)
                        {
                            Console.WriteLine("1st");
                            kp.Dhk = dhk;
                            kp.Registered = true;
                            kp.LastOnlineTime = now;
                            kp.CreatedTime = now;
                            this._context.Update(kp);
                            this._context.SaveChanges();
                            goto Logon;
                        }
                    }
                    // 兩種方法都無法登入，則為 移機 或 惡意登入
                    return new LoginReply { Error = LoginErrors.WilfulLoginBehaviour };
                }

            Logon:
                var expiredTime = now.AddHours(1);
                var tokenInstance = ByteString.CopyFrom(Guid.NewGuid().ToByteArray());
                token = new Token
                {
                    ExpiredTime = Timestamp.FromDateTime(expiredTime),
                    Instance = tokenInstance
                };
                this._tokenCache.Set(context.Peer, token, expiredTime);
                return new LoginReply { Token = token };
            });
            
        }
    }
}
