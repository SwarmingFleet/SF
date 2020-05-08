
namespace SwarmingFleet.Broker
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using SwarmingFleet.Broker.DataAccessLayers; 

    public class Broker
    {
        public static async Task Main(string[] args)
        {
            

            //using (var resx = new BrokerContext())
            //{  
            //    foreach (var site in resx.Sites)
            //    {
            //        Console.WriteLine(site.Id + ": " +site.Url);
            //    }
            //}
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
