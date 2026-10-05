using Voidwell.Platform.Api.Clients;
using Voidwell.Platform.Api.Services;

namespace Voidwell.Platform.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        services.AddHttpClient<IDaybreakGamesClient, DaybreakGamesClient>(client =>
            client.BaseAddress = new Uri("http://voidwelldaybreakgames:5000"));
        services.AddHttpClient<IUserManagementClient, UserManagementClient>(client =>
            client.BaseAddress = new Uri("http://voidwellusermanagement:5000"));

        services.AddAutoMapper(cfg => { }, typeof(ServiceCollectionExtensions).Assembly);

        services.AddTransient<IUserHelper, UserHelper>();
        services.AddTransient<ICustomEventService, CustomEventService>();
        services.AddTransient<IBlogPostService, BlogPostService>();

        return services;
    }
}
