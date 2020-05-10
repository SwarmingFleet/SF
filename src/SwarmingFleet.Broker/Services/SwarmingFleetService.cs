
namespace SwarmingFleet.Broker.Services
{
    using Grpc.Core;
    using SwarmingFleet.Broker.DataAccessLayers;
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

        public SwarmingFleetService(BlockingCollection<string> queue, IRepository<Guid, WorkerInfo> repository)
        {
            this._urls = queue;
            this._repository = repository;
        }
         
        private readonly BlockingCollection<string> _urls;
        private readonly IRepository<Guid, WorkerInfo> _repository;

        public override Task<ConnectReply> Connect(ConnectRequest request, ServerCallContext context)
        {
            return Task.Run(async () =>
            {
                var lastLoginTimes = DateTime.UtcNow;
                var brokerTimestamp = lastLoginTimes.Ticks;
                var address = context.Peer;
                var task = new CrawlTask(); 
                var uuid = default(Guid);

                if (!this._repository.Find(x=>x.Address.Equals(address), out var worker))
                {
                    worker = new WorkerInfo(request.Worker, address, lastLoginTimes);
                    uuid = this._repository.Create(worker);
                    while (!context.CancellationToken.IsCancellationRequested)
                    {
                        if(this._urls.TryTake(out var url))
                        {
                            task.Urls.Add(url);
                            break;
                        }
                        await Task.Delay(1000, context.CancellationToken);
                    }
                }

                return new ConnectReply
                {
                    IsConnected = uuid != default,
                    Address = address,
                    Task = task,
                    Timestamp = request.Timestamp,
                    BrokerTimestamp = brokerTimestamp,
                    Uuid = uuid.ToString()
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

                if (!Guid.TryParse(request.Uuid, out var uuid) || !this._repository.FindById(uuid, out _))
                {
                    return new HandleReply
                    {
                        Timestamp = request.Timestamp,
                        BrokerTimestamp = brokerTimestamp,
                        Task = task,
                        Status = false
                    };
                }

                foreach (var u in request.Task.Urls)
                {
                    this._urls.Add(u, context.CancellationToken);
                }

                var url = default(string);
                // 這裡會一直等到可以dequeue(trytake = dequeue)
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
