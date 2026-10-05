using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voidwell.Platform.Data.Repositories;

namespace Voidwell.Platform.Data;

public static class DatabaseExtensions
{
    public static IServiceCollection AddEntityFrameworkContext(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.Get<DatabaseOptions>() ?? new DatabaseOptions();

        services.AddPooledDbContextFactory<VoidwellDbContext>(
            builder => VoidwellDbContext.Configure(builder, options),
            options.PoolSize);

        services.AddSingleton<ICustomEventRepository, CustomEventRepository>();
        services.AddTransient<IBlogPostRepository, BlogPostRepository>();
        services.AddTransient<IBlogPostTagRepository, BlogPostTagRepository>();

        return services;
    }

    public static IApplicationBuilder InitializeDatabases(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var dbContextFactory = serviceScope.ServiceProvider.GetRequiredService<IDbContextFactory<VoidwellDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.Migrate();

        return app;
    }
}
