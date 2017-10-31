using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Voidwell.Data.DBContext;

namespace Voidwell.Data
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddEntityFrameworkContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.AddSingleton(impl => impl.GetRequiredService<IOptions<DatabaseOptions>>().Value);
            services.Configure<DatabaseOptions>(configuration);

            services.AddEntityFrameworkNpgsql();
            services.AddDbContext<VoidwellDbContext>(ServiceLifetime.Scoped);
            services.AddTransient(sp => new Func<VoidwellDbContext>(() => new VoidwellDbContext(sp.GetRequiredService<DatabaseOptions>())));

            return services;
        }
    }
}
