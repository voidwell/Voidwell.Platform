namespace Voidwell.Platform.Clients.DaybreakGames;

public interface IDaybreakGamesClient
{
    Task<object?> GetCombatReport(string? worldId, string? zoneId, DateTime startDate, DateTime endDate);
    Task<object?> GetTerritoryScoreFromDate(string? worldId, string? zoneId, DateTime endDate);
}
