
namespace SwarmingFleet.Broker.Services
{
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using SwarmingFleet.Broker.DAL;
    using SwarmingFleet.Common.Helpers;
    using SwarmingFleet.Collections.Generic;
    using SwarmingFleet.Common;
    using SwarmingFleet.Contracts;
    using SwarmingFleet.DAL;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
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
            if (!this._nonceCache.TryGetValue(context.Peer, out NonceReply nonceReply))
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
            Task<LoginReply> Fail(LoginErrors error, HazardLevels hazardlevel, string remarks, string dhk = null, string spk = null)
            {
                this._context.ConnectionLogs.Add(new ConnectionLog
                {
                    Action = nameof(Login),
                    Endpoint = context.Peer,
                    Spk = spk ?? string.Empty,
                    Dhk = dhk ?? string.Empty,
                    HazardLevel = hazardlevel,
                    Remarks = remarks,
                });
                this._context.SaveChanges();
                return new LoginReply { Error = error }.WithTask();
            }
            
            Task<LoginReply> Pass(Token token, KeyPair kp)
            {
                this._context.ConnectionLogs.Add(new ConnectionLog
                {
                    Action = nameof(Login),
                    Endpoint = context.Peer,
                    Spk = kp.Spk,
                    Dhk = kp.Dhk
                });
                this._context.SaveChanges();
                return new LoginReply { Token = token }.WithTask();
            }

            var dhk = request.Signature.ToBase64();

            // 如果該勞動者端點所對應的權杖能找到，表示重複登入
            if (this._tokenCache.TryGetValue(context.Peer, out Token token))
            {
                // todo: 需關切此勞動者端點登入情形，並記錄登入狀況，在必要時控制防火牆行為 
                return Fail(LoginErrors.RepeatedLogon, HazardLevels.Low, "可能是權杖被盜用，也可能是權杖還沒過期就重新呼叫 Login", dhk);
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
                return Fail(LoginErrors.Unauthorized, HazardLevels.Middle, "隨機碼過期，通常不太可能。隨機碼過期時間為 60 秒"); 
            }

            var now = DateTime.UtcNow;
            var keyPair = default(KeyPair); // 預設金鑰對

            foreach (var k in this._context.KeyPairs)
            {
                // 當雜湊符合
                if (request.Hash.Equals(BrokerSideUtils.ComputeHash(nonceReply.Nonce, request.Nonce, k.Spk)))
                {
                    // 已綁定
                    if (k.Registered)
                    {
                        // 如果能找到對應的裝置硬體金鑰
                        if (k.Dhk.Equals(dhk))
                        { 
                            // 如果該勞動者端點所對應的伺服器預產金鑰能找到，表示重複登入
                            if (this._tokenCache.TryGetValue(k.Spk, out token))
                            {
                                // todo: 需關切此勞動者端點登入情形，並記錄登入狀況，在必要時控制防火牆行為 
                                return Fail(LoginErrors.RepeatedLogon, HazardLevels.Critical, "同台主機上重複的登入，因為Mutex的關係所以不太可能，判斷為惡意登入", k.Dhk, k.Spk);
                            }

                            k.LastOnlineTime = now;
                            keyPair = k;
                            goto Logon;
                        }

                    }
                    // 第一次登入，需綁定資料
                    else
                    {
                        k.Dhk = dhk;
                        k.Registered = true;
                        k.LastOnlineTime = now;
                        keyPair = k;
                        goto Logon;
                    }
                } 
            }

            // 兩種方法都無法登入，則為 移機 或 惡意登入
            return Fail(LoginErrors.Unauthorized, HazardLevels.Critical , "移機或惡意登入", dhk);

        Logon:

            this._context.Update(keyPair);
            this._context.SaveChanges();

            var expiredTime = now.AddRandomMinutes(1..60); 
            token = new Token
            {
                ExpiredTime = Timestamp.FromDateTime(expiredTime),
                Instance = ByteString.CopyFrom(Guid.NewGuid().ToByteArray())
            };

            this._tokenCache.Set(context.Peer, token, expiredTime);
            this._tokenCache.Set(keyPair.Spk, token, expiredTime); 

            return Pass(token, keyPair);
        }
    }
}
