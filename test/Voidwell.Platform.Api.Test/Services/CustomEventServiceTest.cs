using FluentAssertions;
using Moq;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Api.Services;
using Voidwell.Platform.Clients.DaybreakGames;
using Voidwell.Platform.Data.Models;
using Voidwell.Platform.Data.Repositories;
using Xunit;
using ZiggyCreatures.Caching.Fusion;

namespace Voidwell.Platform.Api.Test.Services;

public sealed class CustomEventServiceTest : IDisposable
{
    private readonly Mock<ICustomEventRepository> _repository = new();
    private readonly FusionCache _cache = new(new FusionCacheOptions());
    private readonly Mock<IDaybreakGamesClient> _daybreakGamesClient = new();
    private readonly CustomEventService _subject;

    public CustomEventServiceTest()
    {
        _subject = new CustomEventService(_repository.Object, _cache, _daybreakGamesClient.Object);
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    [Fact]
    public async Task GetAllCustomEventsAsync_ReturnsEventsAndCaches()
    {
        var events = new[] { CreateEvent(1), CreateEvent(2) };
        _repository.Setup(a => a.GetAllCustomEventsAsync(null, 0, -1)).ReturnsAsync(events);

        var result = await _subject.GetAllCustomEventsAsync();

        result.Should().BeEquivalentTo(events);
        var cached = await _cache.TryGetAsync<IEnumerable<CustomEvent>>("customEventlist", token: TestContext.Current.CancellationToken);
        cached.HasValue.Should().BeTrue();
        cached.Value.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task GetAllCustomEventsAsync_DoesNotCache_WhenThereAreNoEvents()
    {
        _repository.Setup(a => a.GetAllCustomEventsAsync(null, 0, -1)).ReturnsAsync([]);

        var result = await _subject.GetAllCustomEventsAsync();

        result.Should().BeEmpty();
        var cached = await _cache.TryGetAsync<IEnumerable<CustomEvent>>("customEventlist", token: TestContext.Current.CancellationToken);
        cached.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllCustomEventsByGameIdAsync_ReturnsCachedEvents()
    {
        var cached = new[] { CreateEvent(1) };
        await _cache.SetAsync<IEnumerable<CustomEvent>>("customEventList_game1", cached, token: TestContext.Current.CancellationToken);

        var result = await _subject.GetAllCustomEventsByGameIdAsync("game1");

        result.Should().BeEquivalentTo(cached);
        _repository.Verify(a => a.GetAllCustomEventsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAllCustomEventsByGameIdAsync_LoadsFromRepositoryAndCaches_WhenNotCached()
    {
        var events = new[] { CreateEvent(1) };
        _repository.Setup(a => a.GetAllCustomEventsAsync("game1", 0, -1)).ReturnsAsync(events);

        var result = await _subject.GetAllCustomEventsByGameIdAsync("game1");

        result.Should().BeEquivalentTo(events);
        var cached = await _cache.TryGetAsync<IEnumerable<CustomEvent>>("customEventList_game1", token: TestContext.Current.CancellationToken);
        cached.Value.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task GetCustomEventAsync_ReturnsCachedDetails()
    {
        var cached = new CustomEventDetails { Id = 5, Name = "Cached" };
        await _cache.SetAsync("customEventLog_5", cached, token: TestContext.Current.CancellationToken);

        var result = await _subject.GetCustomEventAsync(5);

        result.Should().BeEquivalentTo(cached);
        _repository.Verify(a => a.GetCustomEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetCustomEventAsync_ReturnsNullAndDoesNotCache_WhenEventDoesNotExist()
    {
        var result = await _subject.GetCustomEventAsync(5);

        result.Should().BeNull();
        _daybreakGamesClient.Verify(a => a.GetCombatReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        var cached = await _cache.TryGetAsync<CustomEventDetails>("customEventLog_5", token: TestContext.Current.CancellationToken);
        cached.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetCustomEventAsync_BuildsDetailsFromEventAndDaybreakData()
    {
        var customEvent = CreateEvent(5);
        customEvent.Teams = [new CustomEventTeam { CustomEventId = 5, TeamId = "t1", Name = "Team 1" }];
        var log = new object();
        var score = new object();

        _repository.Setup(a => a.GetCustomEventAsync(5)).ReturnsAsync(customEvent);
        _daybreakGamesClient.Setup(a => a.GetCombatReport("server", "map", customEvent.StartDate, customEvent.EndDate)).ReturnsAsync(log);
        _daybreakGamesClient.Setup(a => a.GetTerritoryScoreFromDate("server", "map", customEvent.EndDate)).ReturnsAsync(score);

        var result = await _subject.GetCustomEventAsync(5);

        result.Should().NotBeNull();
        result.Id.Should().Be(5);
        result.Name.Should().Be(customEvent.Name);
        result.ServerId.Should().Be("server");
        result.MapId.Should().Be("map");
        result.StartDate.Should().Be(customEvent.StartDate);
        result.EndDate.Should().Be(customEvent.EndDate);
        result.Log.Should().BeSameAs(log);
        result.Score.Should().BeSameAs(score);
        result.Teams.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new CustomEventTeamModel { TeamId = "t1", Name = "Team 1" });
        var cached = await _cache.TryGetAsync<CustomEventDetails>("customEventLog_5", token: TestContext.Current.CancellationToken);
        cached.Value.Should().BeSameAs(result);
    }

    [Fact]
    public async Task CreateCustomEventAsync_DelegatesToRepository()
    {
        var customEvent = CreateEvent(0);
        _repository.Setup(a => a.CreateCustomEventAsync(customEvent)).ReturnsAsync(customEvent);

        var result = await _subject.CreateCustomEventAsync(customEvent);

        result.Should().BeSameAs(customEvent);
    }

    [Fact]
    public async Task UpdateCustomEventAsync_DelegatesToRepository()
    {
        var customEvent = CreateEvent(3);
        _repository.Setup(a => a.UpdateCustomEventAsync(customEvent)).ReturnsAsync(customEvent);

        var result = await _subject.UpdateCustomEventAsync(3, customEvent);

        result.Should().BeSameAs(customEvent);
    }

    [Fact]
    public async Task DeleteCustomEventAsync_DelegatesToRepository()
    {
        _repository.Setup(a => a.RemoveCustomEventAsync(3)).Returns(Task.CompletedTask);

        await _subject.DeleteCustomEventAsync(3);

        _repository.Verify(a => a.RemoveCustomEventAsync(3), Times.Once);
    }

    private static CustomEvent CreateEvent(int id)
    {
        return new CustomEvent
        {
            Id = id,
            Name = $"Event {id}",
            ServerId = "server",
            MapId = "map",
            StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}
