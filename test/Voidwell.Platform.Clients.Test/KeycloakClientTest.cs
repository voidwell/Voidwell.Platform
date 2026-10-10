using System.Net;
using System.Text;
using FluentAssertions;
using Voidwell.Platform.Clients.Keycloak;
using Xunit;

namespace Voidwell.Platform.Clients.Test;

public sealed class KeycloakClientTest : IDisposable
{
    private readonly List<(HttpMethod Method, string Url)> _requests = [];
    private readonly Dictionary<string, (HttpStatusCode Status, string Body)> _responses = [];
    private readonly RoutingHandler _handler;
    private readonly HttpClient _httpClient;
    private readonly KeycloakClient _subject;

    public KeycloakClientTest()
    {
        _handler = new RoutingHandler(this);
        _httpClient = new HttpClient(_handler) { BaseAddress = new Uri("http://keycloak.test/") };
        _subject = new KeycloakClient(_httpClient, Microsoft.Extensions.Options.Options.Create(new KeycloakOptions
        {
            BaseUrl = "http://keycloak.test/",
            Realm = "voidwell"
        }));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _handler.Dispose();
    }

    [Fact]
    public async Task GetDisplayNameAsync_ReturnsUsernameForUser()
    {
        var userId = Guid.NewGuid();
        AddUser(userId, "someone");

        var result = await _subject.GetDisplayNameAsync(userId);

        result.UserId.Should().Be(userId);
        result.Name.Should().Be("someone");
        var request = _requests.Single();
        request.Method.Should().Be(HttpMethod.Get);
        request.Url.Should().Be($"http://keycloak.test/admin/realms/voidwell/users/{userId}");
    }

    [Fact]
    public async Task GetDisplayNameAsync_ReturnsDisplayNameAttribute_WhenPresent()
    {
        var userId = Guid.NewGuid();
        _responses[$"GET http://keycloak.test/admin/realms/voidwell/users/{userId}"] =
            (HttpStatusCode.OK, $$$"""{"id":"{{{userId}}}","username":"someone","attributes":{"displayName":["Some One"]}}""");

        var result = await _subject.GetDisplayNameAsync(userId);

        result!.Name.Should().Be("Some One");
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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri.ToString();
            lock (_test._requests)
            {
                _test._requests.Add((request.Method, url));
            }

            if (!_test._responses.TryGetValue($"{request.Method} {url}", out var response))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            return Task.FromResult(new HttpResponseMessage(response.Status)
            {
                Content = new StringContent(response.Body, Encoding.UTF8, "application/json")
            });
        }
    }
}
