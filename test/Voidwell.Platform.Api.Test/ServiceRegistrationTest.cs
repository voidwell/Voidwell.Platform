using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voidwell.Platform.Api.Cache;
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
                ["ConnectionString"] = "Host=localhost;Database=test;Username=test;Password=test"
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
