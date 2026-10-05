using System.Net;
using System.Text.Json;
using FluentAssertions;
using Voidwell.Platform.Api.Clients;
using Xunit;

namespace Voidwell.Platform.Api.Test.Clients;

public class DaybreakGamesClientTest
{
    private static readonly Uri _baseAddress = new("http://daybreak.test");

    [Fact]
    public async Task GetCombatReport_PostsRequestAndReturnsResponseBody()
    {
        var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        using var handler = new StubHttpMessageHandler("""{"kills":3}""");
        using var httpClient = new HttpClient(handler) { BaseAddress = _baseAddress };

        var result = await new DaybreakGamesClient(httpClient).GetCombatReport("17", "2", start, end);

        handler.LastRequest.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri.Should().Be(new Uri(_baseAddress, "ps2/combatReport"));
        using var body = JsonDocument.Parse(handler.LastRequestBody);
        body.RootElement.GetProperty("WorldId").GetString().Should().Be("17");
        body.RootElement.GetProperty("ZoneId").GetString().Should().Be("2");
        body.RootElement.GetProperty("StartDate").GetDateTime().Should().Be(start);
        body.RootElement.GetProperty("EndDate").GetDateTime().Should().Be(end);
        ((JsonElement)result).GetProperty("kills").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task GetTerritoryScoreFromDate_PostsRequestAndReturnsResponseBody()
    {
        var end = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        using var handler = new StubHttpMessageHandler("""{"vs":1}""");
        using var httpClient = new HttpClient(handler) { BaseAddress = _baseAddress };

        var result = await new DaybreakGamesClient(httpClient).GetTerritoryScoreFromDate("17", "2", end);

        handler.LastRequest.RequestUri.Should().Be(new Uri(_baseAddress, "ps2/map/territory"));
        using var body = JsonDocument.Parse(handler.LastRequestBody);
        body.RootElement.GetProperty("EndDate").GetDateTime().Should().Be(end);
        ((JsonElement)result).GetProperty("vs").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetCombatReport_Throws_WhenRequestFails()
    {
        using var handler = new StubHttpMessageHandler("{}", HttpStatusCode.BadGateway);
        using var httpClient = new HttpClient(handler) { BaseAddress = _baseAddress };

        var act = () => new DaybreakGamesClient(httpClient).GetCombatReport("17", "2", DateTime.UtcNow, DateTime.UtcNow);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
