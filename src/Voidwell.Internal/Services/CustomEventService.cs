using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Cache;
using Voidwell.Internal.Clients;
using Voidwell.Internal.Data.Models;
using Voidwell.Internal.Data.Repositories;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public class CustomEventService : ICustomEventService
    {
        private readonly ICache _cache;
        private readonly IDaybreakGamesClient _daybreakGamesClient;
        private readonly ICustomEventRepository _customEventRepository;

        public CustomEventService(ICustomEventRepository customEventRepository, ICache cache, IDaybreakGamesClient daybreakGamesClient)
        {
            _customEventRepository = customEventRepository;
            _cache = cache;
            _daybreakGamesClient = daybreakGamesClient;
        }

        public async Task<IEnumerable<CustomEvent>> GetAllCustomEventsAsync()
        {
            var events = await _cache.GetAsync<IEnumerable<CustomEvent>>("customEventlist");
            if (events != null)
            {
                //return events;
            }

            events = await _customEventRepository.GetAllCustomEventsAsync();

            if (events != null && events.Any())
            {
                await _cache.SetAsync("customEventlist", events, TimeSpan.FromMinutes(5));
            }            

            return events;
        }

        public async Task<IEnumerable<CustomEvent>> GetAllCustomEventsByGameIdAsync(string gameId)
        {
            var cacheKey = $"customEventList_{gameId}";

            var events = await _cache.GetAsync<IEnumerable<CustomEvent>>(cacheKey);
            if (events != null)
            {
                return events;
            }

            events = await _customEventRepository.GetAllCustomEventsAsync(gameId);

            if (events != null && events.Any())
            {
                await _cache.SetAsync(cacheKey, events, TimeSpan.FromMinutes(5));
            }

            return events;
        }

        public async Task<CustomEventDetails> GetCustomEventAsync(int eventId)
        {
            var cacheKey = $"customEventLog_{eventId}";

            var customEventDetails = await _cache.GetAsync<CustomEventDetails>(cacheKey);
            if (customEventDetails != null)
            {
                return customEventDetails;
            }

            var customEvent = await _customEventRepository.GetCustomEventAsync(eventId);
            if (customEvent == null)
            {
                return null;
            }

            var reportTask = _daybreakGamesClient.GetCombatReport(customEvent.ServerId, customEvent.MapId, customEvent.StartDate, customEvent.EndDate);
            var territoryTask = _daybreakGamesClient.GetTerritoryScoreFromDate(customEvent.ServerId, customEvent.MapId, customEvent.EndDate);

            await Task.WhenAll(reportTask, territoryTask);

            customEventDetails = new CustomEventDetails
            {
                Id = customEvent.Id,
                Name = customEvent.Name,
                Description = customEvent.Description,
                ServerId = customEvent.ServerId,
                MapId = customEvent.MapId,
                StartDate = customEvent.StartDate,
                EndDate = customEvent.EndDate,
                IsPrivate = customEvent.IsPrivate,
                GameId = customEvent.GameId,
                Teams = customEvent.Teams.Select(t => new CustomEventTeamModel { TeamId = t.TeamId, Name = t.Name }),
                Log = reportTask.Result,
                Score = territoryTask.Result
            };

            await _cache.SetAsync(cacheKey, customEventDetails, TimeSpan.FromMinutes(15));

            return customEventDetails;
        }

        public Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent)
        {
            return _customEventRepository.CreateCustomEventAsync(customEvent);
        }

        public Task<CustomEvent> UpdateCustomEventAsync(int eventId, CustomEvent customEvent)
        {
            return _customEventRepository.UpdateCustomEventAsync(customEvent);
        }

        public Task DeleteCustomEventAsync(int eventId)
        {
            return _customEventRepository.RemoveCustomEventAsync(eventId);
        }
    }
}
