using System.Net;
using System.Text;
using FluentAssertions;
using Voidwell.Platform.Api.Clients;
using Voidwell.Platform.Api.Options;
using Xunit;
using ZiggyCreatures.Caching.Fusion;

namespace Voidwell.Platform.Api.Test.Clients;

public sealed class KeycloakClientTest : IDisposable
{
    private readonly FusionCache _cache = new(new FusionCacheOptions());
    private readonly List<(HttpMethod Method, string Url, string Authorization, string Body)> _requests = [];
    private readonly Dictionary<string, (HttpStatusCode Status, string Body)> _responses = [];
    private readonly RoutingHandler _handler;
    private readonly HttpClient _httpClient;
    private readonly KeycloakClient _subject;

    public KeycloakClientTest()
    {
        _responses["POST http://keycloak.test/realms/voidwell/protocol/openid-connect/token"] =
            (HttpStatusCode.OK, """{"access_token":"admin-token","expires_in":300}""");

        _handler = new RoutingHandler(this);
        _httpClient = new HttpClient(_handler);
        _subject = new KeycloakClient(_httpClient, _cache, Microsoft.Extensions.Options.Options.Create(new KeycloakOptions
        {
            BaseUrl = "http://keycloak.test/",
            Realm = "voidwell",
            ClientId = "platform",
            ClientSecret = "secret"
        }));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _handler.Dispose();
        _cache.Dispose();
    }

    [Fact]
    public async Task GetDisplayNameAsync_ReturnsUsernameForUser()
    {
        var userId = Guid.NewGuid();
        AddUser(userId, "someone");

        var result = await _subject.GetDisplayNameAsync(userId);

        result.UserId.Should().Be(userId);
        result.Name.Should().Be("someone");
        var request = _requests.Single(r => r.Url.Contains("/users/"));
        request.Method.Should().Be(HttpMethod.Get);
        request.Authorization.Should().Be("Bearer admin-token");
    }

    [Fact]
    public async Task GetDisplayNameAsync_RequestsTokenWithClientCredentials()
    {
        var userId = Guid.NewGuid();
        AddUser(userId, "someone");

        await _subject.GetDisplayNameAsync(userId);

        var tokenRequest = _requests.Single(r => r.Url.EndsWith("/openid-connect/token", StringComparison.Ordinal));
        tokenRequest.Body.Should().Contain("grant_type=client_credentials")
            .And.Contain("client_id=platform")
            .And.Contain("client_secret=secret");
    }

    [Fact]
    public async Task GetDisplayNameAsync_ReusesTheCachedToken()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        AddUser(first, "one");
        AddUser(second, "two");

        await _subject.GetDisplayNameAsync(first);
        await _subject.GetDisplayNameAsync(second);

        _requests.Count(r => r.Url.EndsWith("/openid-connect/token", StringComparison.Ordinal)).Should().Be(1);
    }

    [Fact]
    public async Task GetDisplayNameAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _responses[$"GET http://keycloak.test/admin/realms/voidwell/users/{userId}"] = (HttpStatusCode.NotFound, """{"error":"User not found"}""");

        var result = await _subject.GetDisplayNameAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDisplayNameAsync_ThrowsClientResponseException_WhenKeycloakFails()
    {
        var userId = Guid.NewGuid();
        _responses[$"GET http://keycloak.test/admin/realms/voidwell/users/{userId}"] = (HttpStatusCode.InternalServerError, "boom");

        var act = () => _subject.GetDisplayNameAsync(userId);

        var exception = (await act.Should().ThrowAsync<ClientResponseException>()).Which;
        exception.StatusCode.Should().Be(500);
        exception.Content.Should().Be("boom");
    }

    [Fact]
    public async Task GetDisplayNamesAsync_ReturnsFoundUsersAndSkipsMissingOnes()
    {
        var found = Guid.NewGuid();
        var missing = Guid.NewGuid();
        AddUser(found, "found");
        _responses[$"GET http://keycloak.test/admin/realms/voidwell/users/{missing}"] = (HttpStatusCode.NotFound, "{}");

        var result = await _subject.GetDisplayNamesAsync([found, missing, found]);

        result.Should().ContainSingle().Which.Name.Should().Be("found");
        _requests.Count(r => r.Url.EndsWith(found.ToString(), StringComparison.Ordinal)).Should().Be(1);
    }

    [Fact]
    public async Task GetDisplayNameAsync_Throws_WhenKeycloakIsNotConfigured()
    {
        var subject = new KeycloakClient(_httpClient, _cache, Microsoft.Extensions.Options.Options.Create(new KeycloakOptions()));

        var act = () => subject.GetDisplayNameAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private void AddUser(Guid userId, string username)
    {
        _responses[$"GET http://keycloak.test/admin/realms/voidwell/users/{userId}"] =
            (HttpStatusCode.OK, $$"""{"id":"{{userId}}","username":"{{username}}","enabled":true}""");
    }

    private sealed class RoutingHandler : HttpMessageHandler
    {
        private readonly KeycloakClientTest _test;

        public RoutingHandler(KeycloakClientTest test)
        {
            _test = test;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri.ToString();
            var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            lock (_test._requests)
            {
                _test._requests.Add((request.Method, url, request.Headers.Authorization?.ToString(), body));
            }

            if (!_test._responses.TryGetValue($"{request.Method} {url}", out var response))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(response.Status)
            {
                Content = new StringContent(response.Body, Encoding.UTF8, "application/json")
            };
        }
    }
}
