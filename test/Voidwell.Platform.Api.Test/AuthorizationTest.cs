using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Voidwell.Platform.Api.Controllers;
using Voidwell.Platform.Api.Services;
using Xunit;

namespace Voidwell.Platform.Api.Test;

/// <summary>
/// Mirrors the authorization rules that Voidwell.API enforced in front of these endpoints:
/// reads are public, blog writes require Administrator and event writes require Events.
/// </summary>
public class AuthorizationTest : IAsyncDisposable
{
    private const string _postBody = """{"title":"t","markdownContent":"c"}""";
    private const string _eventBody = """{"name":"n"}""";
    private static readonly string _postId = Guid.NewGuid().ToString();

    private readonly IHost _host;
    private readonly HttpClient _client;

    public AuthorizationTest()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddRouting();
                    services.AddControllers().AddApplicationPart(typeof(PostController).Assembly);
                    services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                    services.AddAuthorization();
                    services.AddSingleton(Mock.Of<IBlogPostService>());
                    services.AddSingleton(Mock.Of<ICustomEventService>());
                    services.AddSingleton(Mock.Of<IUserHelper>());
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                }))
            .Build();

        _host.Start();
        _client = _host.GetTestClient();
    }

    public static TheoryData<string, string, string> ProtectedEndpoints => new()
    {
        { "POST", "/post", "Administrator" },
        { "DELETE", $"/post/{_postId}", "Administrator" },
        { "GET", $"/post/edit/{_postId}", "Administrator" },
        { "PUT", $"/post/edit/{_postId}", "Administrator" },
        { "POST", "/gameevent", "Events" },
        { "PUT", "/gameevent/1", "Events" },
        { "DELETE", "/gameevent/1", "Administrator" },
    };

    public static TheoryData<string, string> PublicEndpoints => new()
    {
        { "GET", "/post" },
        { "GET", $"/post/{_postId}" },
        { "GET", "/gameevent" },
        { "GET", "/gameevent/1" },
        { "GET", "/gameevent/game/ps2" },
        { "GET", "/utils/time" },
    };

    [Theory]
    [MemberData(nameof(PublicEndpoints))]
    public async Task PublicEndpoint_AllowsAnonymousCallers(string method, string path)
    {
        var response = await Send(method, path, roles: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized).And.NotBe(HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task ProtectedEndpoint_RejectsAnonymousCallers(string method, string path, string role)
    {
        role.Should().NotBeNullOrEmpty();

        var response = await Send(method, path, roles: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task ProtectedEndpoint_RejectsAuthenticatedCallersWithoutTheRole(string method, string path, string role)
    {
        var otherRoles = new[] { "User", "Administrator", "Events" }.Where(r => r != role);

        var response = await Send(method, path, string.Join(',', otherRoles));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task ProtectedEndpoint_AllowsCallersWithTheRole(string method, string path, string role)
    {
        var response = await Send(method, path, role);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized).And.NotBe(HttpStatusCode.Forbidden);
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        _host.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private async Task<HttpResponseMessage> Send(string method, string path, string roles)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        if (method is "POST" or "PUT")
        {
            var body = path.StartsWith("/post", StringComparison.Ordinal) ? _postBody : _eventBody;
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        if (roles != null)
        {
            request.Headers.Add(TestAuthHandler.RolesHeader, roles);
        }

        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string RolesHeader = "X-Test-Roles";

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(RolesHeader, out var roles))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = roles.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(role => new Claim(ClaimTypes.Role, role))
                .Append(new Claim("sub", Guid.NewGuid().ToString()));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));
        }
    }
}
