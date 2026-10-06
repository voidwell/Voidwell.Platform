using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Voidwell.Platform.Api.Controllers;
using Voidwell.Platform.Api.Services;
using Voidwell.Platform.Api.Swagger;
using Xunit;

namespace Voidwell.Platform.Api.Test;

public class SwaggerTest
{
    [Fact]
    public async Task SwaggerDocument_MarksOnlyAuthorizedOperationsWithSecurity()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["ApplicationName"] = "Voidwell.Platform" })
            .Build();

        using var host = new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers().AddApplicationPart(typeof(PostController).Assembly);
                    services.AddAuthentication();
                    services.AddAuthorization();
                    services.AddApiSwagger(configuration);
                    services.AddSingleton(Mock.Of<IBlogPostService>());
                    services.AddSingleton(Mock.Of<ICustomEventService>());
                    services.AddSingleton(Mock.Of<IUserHelper>());
                })
                .Configure(app =>
                {
                    app.UseSwagger();
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                }))
            .Build();
        await host.StartAsync(TestContext.Current.CancellationToken);

        using var client = host.GetTestClient();
        var json = await client.GetStringAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(json);
        var paths = document.RootElement.GetProperty("paths");
        document.RootElement.GetProperty("info").GetProperty("title").GetString().Should().Be("Voidwell.Platform");
        paths.GetProperty("/post").GetProperty("get").TryGetProperty("security", out _).Should().BeFalse();
        paths.GetProperty("/post").GetProperty("post").TryGetProperty("security", out _).Should().BeTrue();
        paths.GetProperty("/gameevent").GetProperty("get").TryGetProperty("security", out _).Should().BeFalse();
        paths.GetProperty("/gameevent").GetProperty("post").TryGetProperty("security", out _).Should().BeTrue();
    }
}
