using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Voidwell.Internal.Data;
using Voidwell.Internal.Clients;
using Voidwell.Internal.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Linq;
using Voidwell.Cache;
using Microsoft.AspNetCore.Http;

namespace Voidwell.Internal
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

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCache("Voidwell.Internal");
            services.AddEntityFrameworkContext(Configuration);

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.Authority = "http://voidwellauth:5000";
                o.Audience = "voidwell-internal";
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false
                };

                var validator = o.SecurityTokenValidators.OfType<JwtSecurityTokenHandler>().SingleOrDefault();
                validator.InboundClaimTypeMap = new Dictionary<string, string>();
                validator.OutboundClaimTypeMap = new Dictionary<string, string>();
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
