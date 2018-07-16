using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Voidwell.Internal.Data.Repositories;

namespace Voidwell.Internal.Data
{
    public static class DatabaseExtensions
    {
        private static string _migrationAssembly = typeof(DatabaseExtensions).GetTypeInfo().Assembly.GetName().Name;

        public static IServiceCollection AddEntityFrameworkContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEntityFrameworkNpgsql();

            var connectionString = configuration.GetValue<string>("ConnectionString");

            services.AddDbContextPool<VoidwellDbContext>(builder =>
                builder.UseNpgsql(connectionString, b => {
                    b.MigrationsAssembly(_migrationAssembly);
                }), 5);

            services.AddSingleton<IDbContextHelper, DbContextHelper>();
            services.AddSingleton<ICustomEventRepository, CustomEventRepository>();
            services.AddSingleton<IBlogPostRepository, BlogPostRepository>();

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

        public static IEnumerable<T> ExceptBy<T, TKey>(this IEnumerable<T> items, IEnumerable<T> other, Func<T, TKey> getKey)
        {
            return from item in items
                   join otherItem in other on getKey(item)
                   equals getKey(otherItem) into tempItems
                   from temp in tempItems.DefaultIfEmpty()
                   where ReferenceEquals(null, temp) || temp.Equals(default(T))
                   select item;
        }
    }
}
