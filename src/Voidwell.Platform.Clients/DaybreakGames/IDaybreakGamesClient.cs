namespace Voidwell.Platform.Clients.DaybreakGames;

public interface IDaybreakGamesClient
{
    Task<object?> GetCombatReportAsync(string? worldId, string? zoneId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task<object?> GetTerritoryScoreFromDateAsync(string? worldId, string? zoneId, DateTime endDate, CancellationToken cancellationToken);
}
