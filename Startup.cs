using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace monitor
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        Random rnd = new Random();
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                    //logger.LogInformation("/");
                    OpenMessageEventSource.Instance.Root();
                }); 

                endpoints.MapGet("/delay", async context =>
                {
                    var value = await Task.Run(async () =>
                    {
                        int delay = 1000 * rnd.Next(1, 5);
                        await Task.Delay(delay);
                        return delay;
                    });

                    await context.Response.WriteAsync($"delayed {value} secs!");
                    //logger.LogInformation($"/delayed {value} secs!");
                    OpenMessageEventSource.Instance.Delay();
                });

            });
        }
    }
}
