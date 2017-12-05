using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Voidwell.Internal.Clients
{
    public interface IDaybreakGamesClient
    {
        Task<JToken> GetCombatReport(string worldId, string zoneId, DateTime startDate, DateTime endDate);
        Task<JToken> GetTerritoryScoreFromDate(string worldId, string zoneId, DateTime endDate);
    }
}