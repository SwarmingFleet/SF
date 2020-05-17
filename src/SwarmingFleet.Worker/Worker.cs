
namespace SwarmingFleet.Worker
{
    using Grpc.Core;
    using Grpc.Net.Client;
    using System;
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
    using System.Reflection;
    using System.Security.Cryptography;
    using Polly;
    using Google.Protobuf;
    using Version = Contracts.Version;

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
            var polly = Policy.Handle<RpcException>().RetryAsync(5);

            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

            var options = new GrpcChannelOptions
            {
                HttpClient = httpClient,
                //LoggerFactory = this._loggerFactory
            };
              
            var channel = GrpcChannel.ForAddress(endpoint, options);
            var client = new ConnectionServiceClient(channel);

            var nonceRequest = new NonceRequest { Version = WorkerSideUtils.Version };

            var nonceReply = await polly.ExecuteAsync(async () => await client.PreloginAsync(nonceRequest, cancellationToken: cancellationToken));
             
            var cnonce = WorkerSideUtils.GetNonce();
            var hash = WorkerSideUtils.ComputeHash(nonceReply.Nonce, cnonce);

            var loginRequest = new LoginRequest { Signature = WorkerSideUtils.GetSignature(), Nonce = cnonce, Hash = hash };

            var loginReply = await polly.ExecuteAsync(async () => await client.LoginAsync(loginRequest, cancellationToken: cancellationToken));
             

            switch (loginReply.ResponseCase)
            {
                case LoginReply.ResponseOneofCase.Error:
                    Console.WriteLine(loginReply.Error);
                    break;
                case LoginReply.ResponseOneofCase.Token:
                    Console.WriteLine(loginReply.Token.Instance.ToBase64());
                    Console.WriteLine(loginReply.Token.ExpiredTime.ToDateTime());
                    break;
            }
        }

        static Worker()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        static async Task Main(string[] args)
        { 
            var host = new Uri("http://sybilina.cf");

            var worker = new Worker();

            using (var registration = new CancellationTokenRegistration())
            {
                Console.WriteLine("running");
                _ = worker.Run(host, registration.Token);
                 
                Console.ReadKey();
            }
        }
    }
}
