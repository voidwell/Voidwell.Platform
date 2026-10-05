using Voidwell.Platform.Api.Clients;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Data.Models;
using Voidwell.Platform.Data.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace Voidwell.Platform.Api.Services;

public class CustomEventService : ICustomEventService
{
    private readonly IFusionCache _cache;
    private readonly IDaybreakGamesClient _daybreakGamesClient;
    private readonly ICustomEventRepository _customEventRepository;

    public CustomEventService(ICustomEventRepository customEventRepository, IFusionCache cache, IDaybreakGamesClient daybreakGamesClient)
    {
        _customEventRepository = customEventRepository;
        _cache = cache;
        _daybreakGamesClient = daybreakGamesClient;
    }

    public async Task<IEnumerable<CustomEvent>> GetAllCustomEventsAsync()
    {
        var events = await _customEventRepository.GetAllCustomEventsAsync();

        if (events.Any())
        {
            await _cache.SetAsync("customEventlist", events, CacheFor(TimeSpan.FromMinutes(5)));
        }

        return events;
    }

    public async Task<IEnumerable<CustomEvent>> GetAllCustomEventsByGameIdAsync(string gameId)
    {
        var cacheKey = $"customEventList_{gameId}";

        var cached = await _cache.TryGetAsync<IEnumerable<CustomEvent>>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var events = await _customEventRepository.GetAllCustomEventsAsync(gameId);

        if (events.Any())
        {
            await _cache.SetAsync(cacheKey, events, CacheFor(TimeSpan.FromMinutes(5)));
        }

        return events;
    }

    public async Task<CustomEventDetails?> GetCustomEventAsync(int eventId)
    {
        var cacheKey = $"customEventLog_{eventId}";

        var cached = await _cache.TryGetAsync<CustomEventDetails>(cacheKey);
        if (cached.HasValue && cached.Value != null)
        {
            return cached.Value;
        }

        var customEvent = await _customEventRepository.GetCustomEventAsync(eventId);
        if (customEvent == null)
        {
            return null;
        }

        var reportTask = _daybreakGamesClient.GetCombatReport(customEvent.ServerId, customEvent.MapId, customEvent.StartDate, customEvent.EndDate);
        var territoryTask = _daybreakGamesClient.GetTerritoryScoreFromDate(customEvent.ServerId, customEvent.MapId, customEvent.EndDate);

        await Task.WhenAll(reportTask, territoryTask);

        var customEventDetails = new CustomEventDetails
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
            Teams = customEvent.Teams?.Select(t => new CustomEventTeamModel { TeamId = t.TeamId, Name = t.Name }),
            Log = await reportTask,
            Score = await territoryTask
        };

        await _cache.SetAsync(cacheKey, customEventDetails, CacheFor(TimeSpan.FromMinutes(15)));

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

    private static FusionCacheEntryOptions CacheFor(TimeSpan duration)
    {
        return new FusionCacheEntryOptions { Duration = duration };
    }
}
