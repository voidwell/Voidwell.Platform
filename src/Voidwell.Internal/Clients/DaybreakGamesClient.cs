using System;
using System.Net.Http;
using System.Threading.Tasks;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Clients
{
    public class DaybreakGamesClient : IDaybreakGamesClient, IDisposable
    {
        private readonly HttpClient _httpClient;

        public DaybreakGamesClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://voidwelldaybreakgames:5000");
        }

        public async Task<object> GetCombatReport(string worldId, string zoneId, DateTime startDate, DateTime endDate)
        {
            var request = new CombatReportRequest
            {
                WorldId = worldId,
                ZoneId = zoneId,
                StartDate = startDate,
                EndDate = endDate
            };
            var content = JsonContent.FromObject(request);
            var response = await _httpClient.PostAsync("ps2/combatReport", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsObjectAsync<object>();
        }

        public async Task<object> GetTerritoryScoreFromDate(string worldId, string zoneId, DateTime endDate)
        {
            var request = new CombatReportRequest
            {
                WorldId = worldId,
                ZoneId = zoneId,
                EndDate = endDate
            };
            var content = JsonContent.FromObject(request);
            var response = await _httpClient.PostAsync("ps2/map/territory", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsObjectAsync<object>();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
