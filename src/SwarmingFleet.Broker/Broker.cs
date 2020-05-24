
namespace SwarmingFleet.Broker
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using SwarmingFleet.Broker.DAL;
    using SwarmingFleet.Common;
    using SwarmingFleet.Contracts;

    public class Broker
    {
        public static async Task Main(string[] args)
        {    
#if DEBUG
            var opt = new DbContextOptionsBuilder<BrokerContext>().UseSqlite("Data Source=broker.db").Options;
            using (var db = new BrokerContext(opt))
            {
                const string LOCALHOST = "EvrAHJLz1kqGZR1FXSgZMw==";
                if (!db.KeyPairs.Any(kp => kp.Spk == LOCALHOST))
                {
                    db.KeyPairs.Add(new KeyPair { Spk =LOCALHOST, Registered = false });
                    db.SaveChanges();
                }
            }
#endif
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
