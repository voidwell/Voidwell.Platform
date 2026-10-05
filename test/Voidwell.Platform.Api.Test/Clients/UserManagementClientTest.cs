using System.Net;
using System.Text.Json;
using FluentAssertions;
using Voidwell.Platform.Api.Clients;
using Xunit;

namespace Voidwell.Platform.Api.Test.Clients;

public class UserManagementClientTest
{
    private static readonly Uri _baseAddress = new("http://usermanagement.test");

    [Fact]
    public async Task GetDisplayNameAsync_RequestsUserNameAndDeserializesResponse()
    {
        var userId = Guid.NewGuid();
        using var handler = new StubHttpMessageHandler($$"""{"userId":"{{userId}}","name":"Someone"}""");
        using var httpClient = new HttpClient(handler) { BaseAddress = _baseAddress };

        var result = await new UserManagementClient(httpClient).GetDisplayNameAsync(userId);

        handler.LastRequest.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri.Should().Be(new Uri(_baseAddress, $"user/{userId}/name"));
        result.UserId.Should().Be(userId);
        result.Name.Should().Be("Someone");
    }

    [Fact]
    public async Task GetDisplayNamesAsync_PostsBatchRequestAndDeserializesResponse()
    {
        var userIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        using var handler = new StubHttpMessageHandler($$"""[{"userId":"{{userIds[0]}}","name":"One"},{"userId":"{{userIds[1]}}","name":"Two"}]""");
        using var httpClient = new HttpClient(handler) { BaseAddress = _baseAddress };

        var result = await new UserManagementClient(httpClient).GetDisplayNamesAsync(userIds);

        handler.LastRequest.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri.Should().Be(new Uri(_baseAddress, "user/names"));
        using var body = JsonDocument.Parse(handler.LastRequestBody);
        body.RootElement.GetProperty("UserIds").EnumerateArray().Select(a => a.GetGuid()).Should().Equal(userIds);
        result.Select(a => a.Name).Should().Equal("One", "Two");
    }

    [Fact]
    public async Task GetDisplayNameAsync_ThrowsClientResponseException_WhenRequestFails()
    {
        using var handler = new StubHttpMessageHandler("boom", HttpStatusCode.InternalServerError);
        using var httpClient = new HttpClient(handler) { BaseAddress = _baseAddress };

        var act = () => new UserManagementClient(httpClient).GetDisplayNameAsync(Guid.NewGuid());

        var exception = (await act.Should().ThrowAsync<ClientResponseException>()).Which;
        exception.StatusCode.Should().Be(500);
        exception.Content.Should().Be("boom");
    }
}
