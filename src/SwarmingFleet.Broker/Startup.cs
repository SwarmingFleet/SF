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
    using Microsoft.AspNetCore.Authentication.Certificate;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
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
            //services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme).AddCertificate();
            services.AddMemoryCache();
            services.AddGrpc();

            services.AddControllers();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, BrokerContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            context.Database.EnsureCreated();

            app.UseAuthentication();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            { 
                endpoints.MapGrpcService<ConnectionService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    } 

}
