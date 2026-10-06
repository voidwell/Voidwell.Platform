using Voidwell.Common.Cache;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Clients.DaybreakGames;
using Voidwell.Platform.Data.Models;
using Voidwell.Platform.Data.Repositories;

namespace Voidwell.Platform.Api.Services;

public class CustomEventService : ICustomEventService
{
    private static readonly TimeSpan _eventListExpiry = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan _eventDetailsExpiry = TimeSpan.FromMinutes(15);

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
        var events = await _customEventRepository.GetAllCustomEventsAsync();

        if (events.Any())
        {
            await _cache.SetAsync("customEventlist", events, _eventListExpiry);
        }

        return events;
    }

    public async Task<IEnumerable<CustomEvent>> GetAllCustomEventsByGameIdAsync(string gameId)
    {
        var cacheKey = $"customEventList_{gameId}";

        IEnumerable<CustomEvent>? cached = null;
        if (await _cache.TryGetAsync<IEnumerable<CustomEvent>>(cacheKey, value => cached = value) && cached != null)
        {
            return cached;
        }

        var events = await _customEventRepository.GetAllCustomEventsAsync(gameId);

        if (events.Any())
        {
            await _cache.SetAsync(cacheKey, events, _eventListExpiry);
        }

        return events;
    }

    public async Task<CustomEventDetails?> GetCustomEventAsync(int eventId)
    {
        var cacheKey = $"customEventLog_{eventId}";

        CustomEventDetails? cached = null;
        if (await _cache.TryGetAsync<CustomEventDetails>(cacheKey, value => cached = value) && cached != null)
        {
            return cached;
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

        await _cache.SetAsync(cacheKey, customEventDetails, _eventDetailsExpiry);

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
