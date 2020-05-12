
namespace SwarmingFleet.Worker
{
    using Grpc.Core;
    using Grpc.Net.Client;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;
    using System.Diagnostics;
    using System.Collections.Generic;
    using SwarmingFleet.Common;
    using static SwarmingFleet.Contracts.ConnectionService;
    using SwarmingFleet.Contracts;
    using Google.Protobuf;
    using System.Reflection;
    using System.Collections;

    public class Worker
    {
        //private readonly ILoggerFactory _loggerFactory; 

        //public Worker()
        //{
        //    //this._loggerFactory = LoggerFactory.Create(logging =>
        //    //{
        //    //    logging.AddConsole();
        //    //    logging.SetMinimumLevel(LogLevel.Debug);
        //    //}); 
        //} 
         

        public async Task Run(Uri endpoint, CancellationToken cancellationToken = default)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

            var options = new GrpcChannelOptions
            {
                HttpClient = httpClient,
                //LoggerFactory = this._loggerFactory
            };

            await Task.Run(async () =>
            {
                var channel = GrpcChannel.ForAddress(endpoint, options);
                var client = new ConnectionServiceClient(channel);

                var nonceRequest = new NonceRequest { };
                var nonceReply = await client.PreloginAsync(nonceRequest);

                var cnonce = WorkerSideUtils.GetNonce();
                var hash = WorkerSideUtils.ComputeHash(nonceReply.Nonce, cnonce);
                var loginRequest = new LoginRequest(new LoginRequest { Nonce = cnonce, Hash = hash });

                var loginReply = await client.LoginAsync(loginRequest);
                switch (loginReply.ResponseCase)
                {
                    case LoginReply.ResponseOneofCase.None:
                        break;
                    case LoginReply.ResponseOneofCase.Error:
                        Console.WriteLine(loginReply.Error);
                        break;
                    case LoginReply.ResponseOneofCase.Token:
                        Console.WriteLine(loginReply.Token.Instance.ToBase64());
                        Console.WriteLine(new DateTime(loginReply.Token.ExpiredTime.Ticks));
                        break;
                }
                
                // 重複登入
                loginReply = await client.LoginAsync(loginRequest);
                switch (loginReply.ResponseCase)
                {
                    case LoginReply.ResponseOneofCase.None:
                        break;
                    case LoginReply.ResponseOneofCase.Error:
                        Console.WriteLine(loginReply.Error);
                        break;
                    case LoginReply.ResponseOneofCase.Token:
                        Console.WriteLine(loginReply.Token.Instance.ToBase64());
                        Console.WriteLine(new DateTime(loginReply.Token.ExpiredTime.Ticks));
                        break;
                }

            }, cancellationToken);
        }

        static Worker()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }
     
        static async Task Main(string[] args)
        { 
            var host = new Uri("http://sybilina.cf");

            var crawler = new Worker();

            using (var registration = new CancellationTokenRegistration())
            {
                _ = crawler.Run(host, registration.Token);
                Console.ReadKey();
            }
        }
    }

    public static class WorkerSideUtils
    {
        public static ByteString GetNonce()
        {
            var cnonce = Guid.NewGuid().ToByteArray(); 

            return ByteString.CopyFrom(cnonce);
        }

        public static ByteString ComputeHash(ByteString nonce, ByteString cnonce)
        {
            var n = new BitArray(nonce.ToArray());
            var nc = new BitArray(cnonce.ToArray());
            var k = new BitArray(Guid.Parse(Constants.ActivationCode).ToByteArray());
            var r = n.Xor(nc).Xor(k);
            var ns = new byte[r.Count];
            r.CopyTo(ns, 0);
            return ByteString.CopyFrom(ns);
        }
         
    }
}
