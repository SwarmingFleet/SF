
namespace SwarmingFleet.Broker.Services
{
    using Grpc.Core;
    using SwarmingFleet.Collections.Generic;
    using SwarmingFleet.Contracts;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    using ServiceBase = Contracts.Service.ServiceBase;

    public class SwarmingFleetService : ServiceBase
    {
        public SwarmingFleetService(BlockingCollection<string> queue)
        {
            this._urls = queue;
        }

        private readonly Map<Worker, Guid> _workers = new Map<Worker, Guid>();
        private readonly BlockingCollection<string> _urls;

        public override Task<ConnectReply> Connect(ConnectRequest request, ServerCallContext context)
        {

            return Task.Run(async () =>
            {
                var brokerTimestamp = DateTime.UtcNow.Ticks;
                var address = context.Peer;
                var task = new CrawlTask();

                if (!this._workers.TryGetValue(request.Worker, out var uuid))
                {
                    uuid = Guid.NewGuid();

                    if (this._workers.Add(request.Worker, uuid))
                    {
                        Console.WriteLine(uuid);
                        var url = default(string);
                        

                        while (!context.CancellationToken.IsCancellationRequested && !this._urls.TryTake(out url))
                        {
                            await Task.Delay(1000, context.CancellationToken);
                        }
                        task.Urls.Add(url);

                        return new ConnectReply
                        {
                            IsConnected = true,
                            Address = address,
                            Task = task,
                            Timestamp = request.Timestamp,
                            BrokerTimestamp = brokerTimestamp,
                            Uuid = uuid.ToString()
                        };
                    }
                }

                return new ConnectReply
                {
                    IsConnected = false,
                    Address = address,
                    Task = task,
                    Timestamp = request.Timestamp,
                    BrokerTimestamp = brokerTimestamp,
                    Uuid = Guid.Empty.ToString()
                };

            }, context.CancellationToken);
        } 

        public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            var brokerTimestamp = DateTime.UtcNow.Ticks;

            return Task.FromResult(new PingReply 
            {
                BrokerTimestamp = brokerTimestamp,
                Timestamp = request.Timestamp
            });
        }
        public override Task<HandleReply> Handle(HandleRequest request, ServerCallContext context)
        {
            var brokerTimestamp = DateTime.UtcNow.Ticks;
            var task = new CrawlTask();
            return Task.Run(async () =>
            { 
                foreach (var u in request.Task.Urls)
                {
                    this._urls.Add(u, context.CancellationToken);
                }

                var url = default(string);
                while (!context.CancellationToken.IsCancellationRequested && !this._urls.TryTake(out url))
                {
                    await Task.Delay(1000, context.CancellationToken);
                }

                task.Urls.Add(url);
                return new HandleReply
                { 
                    Timestamp = request.Timestamp,
                    BrokerTimestamp = brokerTimestamp,
                    Task = task,
                    Status = true
                }; 
            }, context.CancellationToken); 
        }

         
    }
}
