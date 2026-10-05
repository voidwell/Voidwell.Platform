using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Voidwell.Platform.Api.Clients;
using Xunit;

namespace Voidwell.Platform.Api.Test.Clients;

public class HttpExtensionsTest
{
    private sealed record Payload(int Count, string Label);

    [Fact]
    public async Task ReadAsObjectAsync_IsCaseInsensitive()
    {
        using var content = new StringContent("""{"COUNT":2,"label":"x"}""");

        var result = await content.ReadAsObjectAsync<Payload>();

        result.Should().Be(new Payload(2, "x"));
    }

    [Fact]
    public async Task ReadAsObjectAsync_ReturnsNull_ForJsonNull()
    {
        using var content = new StringContent("null");

        var result = await content.ReadAsObjectAsync<Payload>();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContentAsync_ReturnsDeserializedBody_WhenSuccessful()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"count":5,"label":"ok"}""", Encoding.UTF8, "application/json")
        };

        var result = await response.GetContentAsync<Payload>();

        result.Should().Be(new Payload(5, "ok"));
    }

    [Fact]
    public async Task GetContentAsync_ThrowsWithStatusAndContent_WhenUnsuccessful()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("missing")
        };

        var act = () => response.GetContentAsync<Payload>();

        var exception = (await act.Should().ThrowAsync<ClientResponseException>()).Which;
        exception.StatusCode.Should().Be(404);
        exception.Content.Should().Be("missing");
    }

    [Fact]
    public async Task JsonContent_FromObject_SerializesAsJson()
    {
        using var content = JsonContent.FromObject(new Payload(1, "a"));

        content.Headers.ContentType.MediaType.Should().Be("application/json");
        using var document = JsonDocument.Parse(await content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        document.RootElement.GetProperty("Count").GetInt32().Should().Be(1);
        document.RootElement.GetProperty("Label").GetString().Should().Be("a");
    }
}
