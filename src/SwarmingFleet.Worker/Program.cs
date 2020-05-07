
namespace SwarmingFleet.Worker
{
    using Grpc.Net.Client;
    using SwarmingFleet.Contracts;
    using System;
    using System.Threading.Tasks;
    using ServiceClient = Contracts.Service.ServiceClient;

    class Program
    {
        static async Task Main(string[] args)
        {
            int i = 0;
            while (i++ < 10)
            {
                using var channel = GrpcChannel.ForAddress("https://localhost:5001");

                var c = new ServiceClient(channel);
                var reply = await c.EchoAsync(new PingRequest { Timestamp = DateTime.UtcNow.Ticks });

                var from = new DateTime(reply.Timestamp);
                var to = new DateTime(reply.BrokerTimestamp);
                Console.WriteLine(i);
                Console.WriteLine(from);
                Console.WriteLine(to);
                Console.WriteLine("span: " + (to - from).TotalMilliseconds + "ms");
                
            }
           

            Console.ReadKey();
        }
    }
}
