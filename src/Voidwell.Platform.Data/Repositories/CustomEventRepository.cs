using Microsoft.EntityFrameworkCore;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.Repositories;

public class CustomEventRepository : ICustomEventRepository
{
    private readonly IDbContextFactory<VoidwellDbContext> _dbContextFactory;

    public CustomEventRepository(IDbContextFactory<VoidwellDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CustomEvent?> GetCustomEventAsync(int customEventId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await GetCustomEventAsync(dbContext, customEventId);
    }

    public async Task<IEnumerable<CustomEvent>> GetAllCustomEventsAsync(string? gameId = null, int skip = 0, int limit = -1)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var query = dbContext.CustomEvents
            .Include(a => a.Teams)
            .Skip(skip)
            .OrderByDescending(a => a.StartDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(gameId))
        {
            query = query.Where(a => a.GameId == gameId);
        }

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        return await query.ToListAsync();
    }

    public async Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var dbEvent = await dbContext.CustomEvents.AddAsync(customEvent);
        await dbContext.SaveChangesAsync();

        return dbEvent.Entity;
    }

    public async Task<CustomEvent> UpdateCustomEventAsync(CustomEvent customEvent)
    {
        var updatedTeams = customEvent.Teams
            ?? throw new ArgumentException("Teams must be provided when updating a custom event.", nameof(customEvent));

        using var dbContext = _dbContextFactory.CreateDbContext();

        var oldCustomEvent = await GetCustomEventAsync(customEvent.Id)
            ?? throw new InvalidOperationException($"Custom event {customEvent.Id} does not exist.");
        var oldTeams = (oldCustomEvent.Teams ?? []).ToList();

        var addedTeams = updatedTeams.ExceptBy(oldTeams, a => a.TeamId).ToList();
        var deletedTeams = oldTeams.ExceptBy(updatedTeams, a => a.TeamId).ToList();

        deletedTeams.ForEach(a => dbContext.Entry(a).State = EntityState.Deleted);
        addedTeams.ForEach(a => dbContext.Entry(a).State = EntityState.Added);

        var dbEvent = dbContext.CustomEvents.Update(customEvent);
        await dbContext.SaveChangesAsync();

        return dbEvent.Entity;
    }

    public async Task RemoveCustomEventAsync(int customEventId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var customEvent = await GetCustomEventAsync(dbContext, customEventId);
        if (customEvent == null)
        {
            return;
        }

        dbContext.CustomEvents.Remove(customEvent);
    }

    private static Task<CustomEvent?> GetCustomEventAsync(VoidwellDbContext dbContext, int customEventId)
    {
        return dbContext.CustomEvents
            .Include(a => a.Teams)
            .SingleOrDefaultAsync(a => a.Id == customEventId);
    }
}
