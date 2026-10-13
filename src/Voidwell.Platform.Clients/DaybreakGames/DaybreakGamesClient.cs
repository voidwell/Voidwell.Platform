using Voidwell.Platform.Clients.DaybreakGames.Models;

namespace Voidwell.Platform.Clients.DaybreakGames;

public class DaybreakGamesClient(HttpClient httpClient) : IDaybreakGamesClient
{
    public async Task<object?> GetCombatReportAsync(string? worldId, string? zoneId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var request = new CombatReportRequest
        {
            WorldId = worldId,
            ZoneId = zoneId,
            StartDate = startDate,
            EndDate = endDate
        };

        using var content = JsonContent.FromObject(request);
        var response = await httpClient.PostAsync("ps2/combatReport", content, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsObjectAsync<object>();
    }

    public async Task<object?> GetTerritoryScoreFromDateAsync(string? worldId, string? zoneId, DateTime endDate, CancellationToken cancellationToken)
    {
        var request = new CombatReportRequest
        {
            WorldId = worldId,
            ZoneId = zoneId,
            EndDate = endDate
        };

        using var content = JsonContent.FromObject(request);
        var response = await httpClient.PostAsync("ps2/map/territory", content, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsObjectAsync<object>();
    }
}
