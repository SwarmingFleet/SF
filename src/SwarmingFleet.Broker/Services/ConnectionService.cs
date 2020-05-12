
namespace SwarmingFleet.Broker.Services
{
    using Google.Protobuf;
    using Grpc.Core;
    using Microsoft.Extensions.Caching.Memory;
    using SwarmingFleet.Broker.DAL;
    using SwarmingFleet.Collections.Generic;
    using SwarmingFleet.Contracts;
    using SwarmingFleet.DAL;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static SwarmingFleet.Contracts.ConnectionService;

    public class ConnectionService : ConnectionServiceBase
    {
        private readonly IMemoryCache _nonceCache;
        private readonly IMemoryCache _tokenCache;

        public ConnectionService(IMemoryCache nonceCache, IMemoryCache tokenCache)
        {
            this._nonceCache = nonceCache;
            this._tokenCache = tokenCache;
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
                    ExpiredTime = new Timestamp
                    {
                        Ticks = expiredTime.Ticks
                    },
                    Nonce = nonce
                };
                this._nonceCache.Set(context.Peer, nonceReply, expiredTime);
            }

            return Task.FromResult(nonceReply);
        }

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        { 
            // 如果該勞動者端點所對應的權杖能找到，表示重複登入
            if(this._tokenCache.TryGetValue(context.Peer, out Token token))
            {
                // todo: 需關切此勞動者端點登入情形，並記錄登入狀況，在必要時控制防火牆行為

                return Task.FromResult(new LoginReply { Error = LoginErrors.RepeatedLogon });
            }

            // 如果能找到隨機碼，表示隨機碼還沒過期
            if(this._nonceCache.TryGetValue(context.Peer, out NonceReply nonceReply))
            {
                // 移除快取中的隨機碼
                this._nonceCache.Remove(context.Peer);
            }
            // 隨機碼過期，需要重新呼叫 Prelogin
            else
            {
                return Task.FromResult(new LoginReply { Error = LoginErrors.NonceExpired });
            }

            var activationCode = "C6C04B99-4BD0-41B8-B726-760D7DD6E07C"; //account
            var key = ByteString.Empty; // hardware info
            var hash = BrokerSideUtils.ComputeHash(nonceReply.Nonce, request.Nonce, activationCode);

            // 如果雜湊相符，建立新權杖交付給勞動者端點
            if (request.Hash.Equals(hash))
            { 
                var now = DateTime.UtcNow;
                var expiredTime = now.AddHours(1);
                var tokenInstance = ByteString.CopyFrom(Guid.NewGuid().ToByteArray());
                token = new Token
                {
                    ExpiredTime = new Timestamp
                    {
                        Ticks = expiredTime.Ticks
                    },
                    Instance = tokenInstance
                };
                this._tokenCache.Set(context.Peer, token, expiredTime);
                return Task.FromResult(new LoginReply { Token = token });
            }
            // 雜湊不相符，可能有外力介入，需關切該勞動者端點行為
            else
            {
                return Task.FromResult(new LoginReply { Error = LoginErrors.HashUnmatched });
            }
        }
    }
}
