#define USE_EF_DB

namespace SwarmingFleet.Broker
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using SwarmingFleet.Broker.DAL;
    using SwarmingFleet.Broker.Services;
    using SwarmingFleet.DAL;

    public class Startup
    { 
        public void ConfigureServices(IServiceCollection services)
        {
#if USE_EF_DB
            services.AddDbContext<BrokerContext>(options => options.UseSqlite("Data Source=broker.db"));
#endif

            services.AddMemoryCache(); 
            services.AddGrpc();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, BrokerContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            context.Database.EnsureCreated(); 


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
               

                endpoints.MapGrpcService<ConnectionService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("gRPC 服務已啟用"); 
                });
                
                endpoints.MapGet("/index", async context =>
                {
                    await context.Response.SendFileAsync(new S());
                });
            });
        }
    }

    class S : IFileInfo
    {
        public Stream CreateReadStream()
        {
            return new MemoryStream();
        }

        public bool Exists => true;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;

        public long Length => throw new NotImplementedException();

        public string Name => "Test.txt";

        public string PhysicalPath => throw new NotImplementedException();
    }
}
