using Voidwell.Platform.Api.Services;
using Voidwell.Platform.Clients;

namespace Voidwell.Platform.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        services.AddPlatformClients();

        services.AddAutoMapper(cfg => { }, typeof(ServiceCollectionExtensions).Assembly);

        services.AddTransient<IUserHelper, UserHelper>();
        services.AddTransient<ICustomEventService, CustomEventService>();
        services.AddTransient<IBlogPostService, BlogPostService>();

        return services;
    }
}
