using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Voidwell.Data;
using Newtonsoft.Json.Serialization;
using Voidwell.Cache;
using Voidwell.Services;
using Voidwell.Clients;

namespace Voidwell
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddDataAnnotations()
                .AddJsonFormatters(options =>
                {
                    options.NullValueHandling = NullValueHandling.Ignore;
                    options.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.AddCache("Voidwell");
            services.AddEntityFrameworkContext(Configuration);

            services.AddSingleton<IDaybreakGamesClient, DaybreakGamesClient>();

            services.AddTransient<ICustomEventService, CustomEventService>();
            services.AddTransient<IBlogService, BlogService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory
                .WithFilter(new FilterLoggerSettings
                {
                    { "Microsoft", LogLevel.Error }
                })
                .AddConsole(Configuration.GetSection("Logging"))
                .AddDebug();

            app.UseMvc();
        }
    }
}
