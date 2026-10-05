using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Voidwell.Platform.Api.Cache;
using Xunit;
using ZiggyCreatures.Caching.Fusion;

namespace Voidwell.Platform.Api.Test.Cache;

public class CacheRegistrationTest
{
    [Fact]
    public async Task AddCache_WithoutRedis_UsesMemoryOnly()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCache(options => options.KeyPrefix = "test:");

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IFusionCache>();

        var value = await cache.GetOrSetAsync("key", _ => Task.FromResult("value"), token: TestContext.Current.CancellationToken);

        value.Should().Be("value");
        cache.HasDistributedCache.Should().BeFalse();
    }

    [Fact]
    public async Task AddCache_WithUnreachableRedis_StillServesFromMemory()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCache(options =>
        {
            options.KeyPrefix = "test:";
            options.RedisConfiguration = "127.0.0.1:1,abortConnect=false,connectTimeout=500,syncTimeout=500,asyncTimeout=500";
        });

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IFusionCache>();
        var calls = 0;

        Task<string> Factory(CancellationToken ct)
        {
            calls++;
            return Task.FromResult("value");
        }

        var first = await cache.GetOrSetAsync<string>("key", (_, ct) => Factory(ct), token: TestContext.Current.CancellationToken);
        var second = await cache.GetOrSetAsync<string>("key", (_, ct) => Factory(ct), token: TestContext.Current.CancellationToken);

        first.Should().Be("value");
        second.Should().Be("value");
        calls.Should().Be(1);
        cache.HasDistributedCache.Should().BeTrue();
    }
}
