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

    private const string _customEventsCacheKey = "customEventlist";
    private readonly Func<string, string> _getCustomEventsGameCacheKey = gameId => $"{_customEventsCacheKey}_{gameId}";
    private readonly Func<int, string> _getCustomEventLogCacheKey = eventId => $"customEventLog_{eventId}";

    private readonly ICache _cache;
    private readonly IDaybreakGamesClient _daybreakGamesClient;
    private readonly ICustomEventRepository _customEventRepository;

    public CustomEventService(ICustomEventRepository customEventRepository, ICache cache, IDaybreakGamesClient daybreakGamesClient)
    {
        _customEventRepository = customEventRepository;
        _cache = cache;
        _daybreakGamesClient = daybreakGamesClient;
    }

    public async Task<IEnumerable<CustomEvent>> GetCustomEventsAsync(CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            _customEventsCacheKey,
            ct => _customEventRepository.GetCustomEventsAsync(cancellationToken),
            _eventListExpiry,
            e => e.Any(),
            cancellationToken);
    }

    public async Task<IEnumerable<CustomEvent>> GetCustomEventsAsync(string gameId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            _getCustomEventsGameCacheKey(gameId),
            ct => _customEventRepository.GetCustomEventsAsync(gameId, cancellationToken),
            _eventListExpiry,
            e => e.Any(),
            cancellationToken);
    }

    public async Task<CustomEventDetails?> GetCustomEventAsync(int eventId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetIfNotNullAsync(
            _getCustomEventLogCacheKey(eventId),
            ct => GetCustomEventDataAsync(eventId, ct),
            _eventDetailsExpiry,
            cancellationToken
        );
    }

    private async Task<CustomEventDetails?> GetCustomEventDataAsync(int eventId, CancellationToken cancellationToken)
    {
        var customEvent = await _customEventRepository.GetCustomEventAsync(eventId, cancellationToken);
        if (customEvent == null)
        {
            return null;
        }

        var reportTask = _daybreakGamesClient.GetCombatReportAsync(customEvent.ServerId, customEvent.MapId, customEvent.StartDate, customEvent.EndDate, cancellationToken);
        var territoryTask = _daybreakGamesClient.GetTerritoryScoreFromDateAsync(customEvent.ServerId, customEvent.MapId, customEvent.EndDate, cancellationToken);

        await Task.WhenAll(reportTask, territoryTask);

        var teams = customEvent.Teams?.Select(t => new CustomEventTeamModel
        {
            TeamId = t.TeamId,
            Name = t.Name
        });

        return new CustomEventDetails
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
            Teams = teams,
            Log = await reportTask,
            Score = await territoryTask
        };
    }

    public async Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent, CancellationToken cancellationToken)
    {
        var result = await _customEventRepository.CreateCustomEventAsync(customEvent, cancellationToken);

        await ClearEventCache(result);

        return result;
    }

    public async Task<CustomEvent> UpdateCustomEventAsync(int eventId, CustomEvent customEvent, CancellationToken cancellationToken)
    {
        var result = await _customEventRepository.UpdateCustomEventAsync(customEvent, cancellationToken);
        
        await ClearEventCache(result);

        return result;
    }

    public async Task DeleteCustomEventAsync(int eventId, CancellationToken cancellationToken)
    {
        var customEvent = await _customEventRepository.GetCustomEventAsync(eventId, cancellationToken);
        if (customEvent == null)
        {
            return;
        }

        await _customEventRepository.RemoveCustomEventAsync(customEvent, cancellationToken);
        await ClearEventCache(customEvent);
    }

    private async Task ClearEventCache(CustomEvent customEvent)
    {
        await _cache.RemoveAsync(_customEventsCacheKey);
        await _cache.RemoveAsync(_getCustomEventLogCacheKey(customEvent.Id));

        if (customEvent.GameId != null)
        {
            await _cache.RemoveAsync(_getCustomEventsGameCacheKey(customEvent.GameId));
        }
    }
}
