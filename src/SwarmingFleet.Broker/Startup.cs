using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwarmingFleet.Broker.DAL; 
using SwarmingFleet.Broker.Services;
using SwarmingFleet.DAL;

namespace SwarmingFleet.Broker
{
    public class Startup
    { 
        public void ConfigureServices(IServiceCollection services)
        {
            var queue = new BlockingCollection<string>();
            // 入口
            queue.Add("https://google.com?q=orlys");
            queue.Add("https://en.wikipedia.org/wiki/Web_crawler");


            services.AddSingleton(queue);

            services.AddDbContext<BrokerContext>(options =>
            {
                options.UseSqlite("Data Source=broker.db");
            });

            services.AddScoped<IRepository<Guid, WorkerInfo>, Repository<BrokerContext, Guid, WorkerInfo>>();

            services.AddGrpc();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, BrokerContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            dbContext.Database.EnsureCreated();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<SwarmingFleetService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
