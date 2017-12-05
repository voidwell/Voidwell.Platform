using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Voidwell.Internal.Clients
{
    public class DaybreakGamesClient : IDaybreakGamesClient, IDisposable
    {
        private readonly HttpClient _httpClient;

        public DaybreakGamesClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://daybreakgames:5000");
        }

        public async Task<JToken> GetCombatReport(string worldId, string zoneId, DateTime startDate, DateTime endDate)
        {
            var combatReport = await _httpClient.GetAsync("ps2/combatReport/{serverId}/{mapId}/{startDate}/{endDate}");
            var content = await combatReport.Content.ReadAsStringAsync();
            return JToken.FromObject(content);
        }

        public async Task<JToken> GetTerritoryScoreFromDate(string worldId, string zoneId, DateTime endDate)
        {
            var combatReport = await _httpClient.GetAsync("ps2/map/territory{serverId}/{mapId}/{startDate}/{endDate}");
            var content = await combatReport.Content.ReadAsStringAsync();
            return JToken.FromObject(content);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
