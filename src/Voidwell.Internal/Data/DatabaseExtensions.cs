using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection;
using System;

namespace Voidwell.Internal.Data
{
    public static class DatabaseExtensions
    {
        private static string _migrationAssembly = typeof(DatabaseExtensions).GetTypeInfo().Assembly.GetName().Name;

        public static IServiceCollection AddEntityFrameworkContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.AddSingleton(impl => impl.GetRequiredService<IOptions<DatabaseOptions>>().Value);
            services.Configure<DatabaseOptions>(configuration);

            var options = configuration.Get<DatabaseOptions>();

            services.AddEntityFrameworkNpgsql();

            services.AddDbContext<VoidwellDbContext>(builder =>
                builder.UseNpgsql(options.DBConnectionString, b => {
                    b.MigrationsAssembly(_migrationAssembly);
                    //b.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null);
                }));
            services.AddTransient(sp => new Func<VoidwellDbContext>(() => sp.GetRequiredService<VoidwellDbContext>()));

            return services;
        }

        public static IApplicationBuilder InitializeDatabases(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<VoidwellDbContext>();
                dbContext.Database.Migrate();
            }

            return app;
        }
    }
}
