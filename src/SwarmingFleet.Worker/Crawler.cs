
namespace SwarmingFleet.Worker
{
    using Grpc.Core;
    using Grpc.Net.Client;
    using SwarmingFleet.Contracts;
    using System;
    using System.Linq;
    using System.Management;
    using System.Net.Http;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using ServiceClient = Contracts.Service.ServiceClient;
    using Microsoft.Extensions.Logging;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;
    using System.Diagnostics;
    using System.Collections.Generic;

    public class Crawler
    {
        private Worker BuildWorkerObject()
        {
            var memSize = (ulong)(new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory")
          .Get().Cast<ManagementObject>()
          .Sum(mo => (double)(ulong)mo["Capacity"]) / 1048576);

            var macs = from nic in NetworkInterface.GetAllNetworkInterfaces()
                       where nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                       select nic.GetPhysicalAddress().ToString();

            var processors = new ManagementObjectSearcher("SELECT * FROM Win32_Processor")
                .Get().Cast<ManagementObject>()
                .Select(mo => (string)mo["ProcessorId"]);

            var worker = new Contracts.Worker
            {
                MemorySize = memSize,
                OperationSystem = RuntimeInformation.OSDescription,
                Name = Environment.MachineName
            };
            worker.Processors.AddRange(processors);
            worker.MacAddresses.AddRange(macs);
            return worker;
        }

        private readonly ILoggerFactory _loggerFactory;

        public Worker Worker { get; }

        public Crawler()
        {
            //this._loggerFactory = LoggerFactory.Create(logging =>
            //{
            //    logging.AddConsole();
            //    logging.SetMinimumLevel(LogLevel.Debug);
            //});

            this.Worker = this.BuildWorkerObject();
        }

        private static readonly CrawlTask s_none = new CrawlTask();

        public async Task<(TimeSpan upload, TimeSpan download)?> PingAsync(Uri endpoint)
        {
            var options = new GrpcChannelOptions
            {
                HttpClient = new HttpClient(),
                LoggerFactory = this._loggerFactory
            };

            var channel = GrpcChannel.ForAddress(endpoint, options);
            var client = new ServiceClient(channel);

            // 連線請求
            var connectRequest = new ConnectRequest
            {
                Timestamp = DateTime.UtcNow.Ticks,
                Worker = this.Worker
            };
            var connectReply = await client.ConnectAsync(connectRequest);
            if (!connectReply.IsConnected)
                await Task.FromException(new RpcException(Status.DefaultCancelled));
            else
            {
                var pingRequest = new PingRequest { Timestamp = DateTime.UtcNow.Ticks };
                var pingReply = await client.PingAsync(pingRequest);
                var now = DateTime.UtcNow.Ticks;

                var upload = new TimeSpan(pingReply.BrokerTimestamp - pingReply.Timestamp);
                var download = new TimeSpan(now - pingReply.BrokerTimestamp);
                return (upload, download);
            }
            return null;
        }

        public async Task Run(Uri endpoint, CancellationToken cancellationToken = default)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

            var options = new GrpcChannelOptions
            {
                HttpClient = httpClient,
                LoggerFactory = this._loggerFactory
            };

            await Task.Run(async () =>
            {
                var channel = GrpcChannel.ForAddress(endpoint, options);
                var client = new ServiceClient(channel); 
                // 連線請求
                var connectRequest = new ConnectRequest
                {
                    Timestamp = DateTime.UtcNow.Ticks,
                    Worker = this.Worker
                };
                var connectReply = await client.ConnectAsync(connectRequest, cancellationToken: cancellationToken);
                if (!connectReply.IsConnected)
                    await Task.FromException(new RpcException(Status.DefaultCancelled));
                else
                { 
                    var target = connectReply.Task.Urls[0];

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("crawling: " + target); 
                        var task = s_none;
                        var get = default(HttpResponseMessage);
                        try
                        { 
                            get = await httpClient.GetAsync(target);
                            var doc = new HtmlDocument();
                            var content = await get.Content.ReadAsStreamAsync();
                            doc.Load(content);
                             
                            if(doc.DocumentNode.SelectNodes("//a") is HtmlNodeCollection nodes)
                            {
                                var hrefs = (from node in nodes
                                             let p = node.GetAttributeValue("href", null)
                                             where p != null && p.StartsWith("http")
                                             select p).ToArray();
                                if(hrefs.Length > 0)
                                {
                                    task = new CrawlTask();
                                    task.Urls.AddRange(hrefs);
                                }
                            } 

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("raised: " + e); 
                        }

                        var now = DateTime.UtcNow;
                        var handleRequest = new HandleRequest
                        {
                            Task = task,
                            Timestamp = now.Ticks,
                            Uuid = connectReply.Uuid
                        }; 
                        var handleReply = await client.HandleAsync(handleRequest);
                        target = handleReply.Task.Urls[0];
                    }
                }
            }, cancellationToken);
        }
     
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var host = new Uri("http://dev.orlys.site");

            var crawler = new Crawler(); 

            using (var registration = new CancellationTokenRegistration())
            { 
                _ = crawler.Run(host, registration.Token);
                Console.ReadKey(); 
            } 
        }
    }
}
