using Voidwell.Platform.Api.Models;

namespace Voidwell.Platform.Api.Clients;

public class DaybreakGamesClient : IDaybreakGamesClient
{
    private readonly HttpClient _httpClient;

    public DaybreakGamesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<object?> GetCombatReport(string? worldId, string? zoneId, DateTime startDate, DateTime endDate)
    {
        var request = new CombatReportRequest
        {
            WorldId = worldId,
            ZoneId = zoneId,
            StartDate = startDate,
            EndDate = endDate
        };
        using var content = JsonContent.FromObject(request);
        var response = await _httpClient.PostAsync("ps2/combatReport", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsObjectAsync<object>();
    }

    public async Task<object?> GetTerritoryScoreFromDate(string? worldId, string? zoneId, DateTime endDate)
    {
        var request = new CombatReportRequest
        {
            WorldId = worldId,
            ZoneId = zoneId,
            EndDate = endDate
        };
        using var content = JsonContent.FromObject(request);
        var response = await _httpClient.PostAsync("ps2/map/territory", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsObjectAsync<object>();
    }
}
