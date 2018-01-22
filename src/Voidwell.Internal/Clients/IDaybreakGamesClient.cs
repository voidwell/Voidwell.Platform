using System;
using System.Threading.Tasks;

namespace Voidwell.Internal.Clients
{
    public interface IDaybreakGamesClient
    {
        Task<object> GetCombatReport(string worldId, string zoneId, DateTime startDate, DateTime endDate);
        Task<object> GetTerritoryScoreFromDate(string worldId, string zoneId, DateTime endDate);
    }
}