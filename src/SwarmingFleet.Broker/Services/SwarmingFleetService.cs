
namespace SwarmingFleet.Broker.Services
{
    using Grpc.Core;
    using SwarmingFleet.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ServiceBase = Contracts.Service.ServiceBase;

    public class SwarmingFleetService : ServiceBase
    {
        public override Task<AccessControlReply> Connect(AccessControlRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AccessControlReply
            {
                State = AccessControlReply.Types.State.Connected,
                Timestamp = request.Timestamp
            });
        }
        public override Task<AccessControlReply> Disconnect(AccessControlRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AccessControlReply
            {
                State = AccessControlReply.Types.State.Disconnected,
                Timestamp = request.Timestamp
            });
        }
        public override Task<PingReply> Echo(PingRequest request, ServerCallContext context)
        {  
            return Task.FromResult(new PingReply 
            {
                BrokerTimestamp = DateTime.UtcNow.Ticks,
                Timestamp = request.Timestamp
            });
        }
    }
}
