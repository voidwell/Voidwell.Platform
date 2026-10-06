using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voidwell.Common.Cache;
using Voidwell.Platform.Api.Controllers;
using Voidwell.Platform.Api.Services;
using Voidwell.Platform.Data;
using Xunit;

namespace Voidwell.Platform.Api.Test;

public class ServiceRegistrationTest
{
    [Fact]
    public void ControllersAndServices_CanBeResolved()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionString"] = "Host=localhost;Database=test;Username=test;Password=test",
                ["DaybreakGames:ClientId"] = "daybreak",
                ["DaybreakGames:ClientSecret"] = "secret",
                ["DaybreakGames:TokenServiceAddress"] = "http://auth.test/token",
                ["Keycloak:BaseUrl"] = "http://keycloak.test",
                ["Keycloak:Realm"] = "voidwell",
                ["Keycloak:ClientId"] = "platform",
                ["Keycloak:ClientSecret"] = "secret",
                ["Keycloak:TokenServiceAddress"] = "http://keycloak.test/token"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddHttpContextAccessor();
        services.AddCache(options => options.KeyPrefix = "test");
        services.AddEntityFrameworkContext(configuration);
        services.AddPlatformServices();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<ICustomEventService>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IBlogPostService>().Should().NotBeNull();
        var postController = ActivatorUtilities.CreateInstance<PostController>(scope.ServiceProvider);
        using var customEventController = ActivatorUtilities.CreateInstance<CustomEventController>(scope.ServiceProvider);

        postController.Should().NotBeNull();
        customEventController.Should().NotBeNull();
    }
}
