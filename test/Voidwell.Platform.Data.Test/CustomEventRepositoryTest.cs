using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voidwell.Platform.Data.Models;
using Voidwell.Platform.Data.Repositories;
using Xunit;

namespace Voidwell.Platform.Data.Test;

public class CustomEventRepositoryTest
{
    private readonly TestDbContextFactory _factory = new();
    private readonly CustomEventRepository _subject;

    public CustomEventRepositoryTest()
    {
        _subject = new CustomEventRepository(_factory);
    }

    [Fact]
    public async Task CreateCustomEventAsync_PersistsEventWithTeams()
    {
        var created = await _subject.CreateCustomEventAsync(new CustomEvent
        {
            Name = "Event",
            GameId = "game",
            Teams = [new CustomEventTeam { TeamId = "t1", Name = "Team 1" }]
        });

        created.Id.Should().BeGreaterThan(0);

        var loaded = await _subject.GetCustomEventAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded.Name.Should().Be("Event");
        loaded.Teams.Should().ContainSingle().Which.TeamId.Should().Be("t1");
    }

    [Fact]
    public async Task GetCustomEventAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        var result = await _subject.GetCustomEventAsync(123);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCustomEventsAsync_ReturnsEventsNewestFirst()
    {
        await _subject.CreateCustomEventAsync(new CustomEvent { Name = "Old", StartDate = new DateTime(2024, 1, 1) });
        await _subject.CreateCustomEventAsync(new CustomEvent { Name = "New", StartDate = new DateTime(2024, 6, 1) });

        var result = await _subject.GetAllCustomEventsAsync();

        result.Select(a => a.Name).Should().Equal("New", "Old");
    }

    [Fact]
    public async Task GetAllCustomEventsAsync_FiltersByGameId()
    {
        await _subject.CreateCustomEventAsync(new CustomEvent { Name = "A", GameId = "game1" });
        await _subject.CreateCustomEventAsync(new CustomEvent { Name = "B", GameId = "game2" });

        var result = await _subject.GetAllCustomEventsAsync("game2");

        result.Should().ContainSingle().Which.Name.Should().Be("B");
    }

    [Fact]
    public async Task GetAllCustomEventsAsync_AppliesLimit()
    {
        await _subject.CreateCustomEventAsync(new CustomEvent { Name = "A", StartDate = new DateTime(2024, 1, 1) });
        await _subject.CreateCustomEventAsync(new CustomEvent { Name = "B", StartDate = new DateTime(2024, 2, 1) });

        var result = await _subject.GetAllCustomEventsAsync(limit: 1);

        result.Should().ContainSingle().Which.Name.Should().Be("B");
    }

    [Fact]
    public async Task UpdateCustomEventAsync_UpdatesFieldsAndDiffsTeams()
    {
        var created = await _subject.CreateCustomEventAsync(new CustomEvent
        {
            Name = "Event",
            Teams = new List<CustomEventTeam>
            {
                new CustomEventTeam { TeamId = "keep", Name = "Keep" },
                new CustomEventTeam { TeamId = "drop", Name = "Drop" }
            }
        });

        await _subject.UpdateCustomEventAsync(new CustomEvent
        {
            Id = created.Id,
            Name = "Renamed",
            Teams = new List<CustomEventTeam>
            {
                new CustomEventTeam { CustomEventId = created.Id, TeamId = "keep", Name = "Keep" },
                new CustomEventTeam { CustomEventId = created.Id, TeamId = "add", Name = "Add" }
            }
        });

        var loaded = await _subject.GetCustomEventAsync(created.Id);
        loaded.Name.Should().Be("Renamed");
        loaded.Teams.Select(a => a.TeamId).Should().BeEquivalentTo("keep", "add");
    }

    [Fact]
    public async Task UpdateCustomEventAsync_Throws_WhenTeamsAreMissing()
    {
        var act = () => _subject.UpdateCustomEventAsync(new CustomEvent { Id = 1, Name = "Event" });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateCustomEventAsync_Throws_WhenEventDoesNotExist()
    {
        var act = () => _subject.UpdateCustomEventAsync(new CustomEvent { Id = 99, Name = "Event", Teams = [] });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RemoveCustomEventAsync_DoesNothing_WhenEventDoesNotExist()
    {
        var act = () => _subject.RemoveCustomEventAsync(99);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Model_UsesSnakeCaseTableNames()
    {
        await using var context = _factory.CreateDbContext();

        context.Model.FindEntityType(typeof(CustomEvent)).GetTableName().Should().Be("custom_event");
        context.Model.FindEntityType(typeof(CustomEventTeam)).GetTableName().Should().Be("custom_event_team");
        (await context.CustomEvents.CountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
    }
}
