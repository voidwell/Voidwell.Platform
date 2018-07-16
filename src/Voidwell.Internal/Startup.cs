using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Voidwell.Internal.Data;
using Voidwell.Internal.Clients;
using Voidwell.Internal.Services;
using Voidwell.Cache;
using Microsoft.AspNetCore.Http;
using IdentityServer4.AccessTokenValidation;

namespace Voidwell.Internal
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .SetBasePath(env.ContentRootPath);

            if (env.IsDevelopment())
            {
                builder.AddJsonFile("devsettings.json", true, true);
            }

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
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
                    options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCache("Voidwell.Internal");
            services.AddEntityFrameworkContext(Configuration);

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://voidwellauth:5000";
                    options.SupportedTokens = SupportedTokens.Jwt;
                    options.RequireHttpsMetadata = false;
                });

            services.AddSingleton<IDaybreakGamesClient, DaybreakGamesClient>();
            services.AddSingleton<IUserManagementClient, UserManagementClient>();

            services.AddTransient<IUserHelper, UserHelper>();
            services.AddTransient<ICustomEventService, CustomEventService>();
            services.AddTransient<IBlogService, BlogService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.InitializeDatabases();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
