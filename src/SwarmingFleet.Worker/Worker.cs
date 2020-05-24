
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
    using System.Net;
    using System.Security.Cryptography.X509Certificates;


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
            var polly = Policy.Handle<RpcException>().RetryAsync(5, (e, i) => Console.WriteLine(e.Message + "\r\n"));
             
            //using var cert = X509Certificate.CreateFromSignedFile(Assembly.GetExecutingAssembly().Location);
            //using var cert2 = new X509Certificate2(cert);

            var handler = new HttpClientHandler
            { 
                ServerCertificateCustomValidationCallback = delegate { return true; }
            }; 

            //handler.ClientCertificates.Add(cert2);
            //handler. ClientCertificateOptions = ClientCertificateOption.Automatic;

            var httpClient = new HttpClient(handler);

            var options = new GrpcChannelOptions
            {
                HttpClient = httpClient,
                //LoggerFactory = this._loggerFactory
            };

           /* var channelCredentials = new SslCredentials( ); */ // Load a custom roots file.
            //var channel = new Channel(host, channelCredentials);

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

#pragma warning disable IDE0052 
        private static readonly Mutex s_mutex = null;
#pragma warning restore IDE0052 

        static Worker()
        {
            var token = Assembly.GetEntryAssembly().GetName().GetPublicKeyToken();
            if (token == null || token.Length == 0)
            { 
                Environment.Exit(0);
            }

            s_mutex = new Mutex(false, Convert.ToBase64String(token), out var createdNew);
            if (!createdNew)
            {
                Environment.Exit(0);
            }

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)4032;
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        static async Task Main(string[] args)
        { 
            var host = new Uri("https://sybilina.cf");

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
