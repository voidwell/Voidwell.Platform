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

    public async Task<CustomEvent?> GetCustomEventAsync(int customEventId, CancellationToken cancellationToken)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.CustomEvents
            .Include(a => a.Teams)
            .SingleOrDefaultAsync(a => a.Id == customEventId, cancellationToken);
    }

    public async Task<IEnumerable<CustomEvent>> GetCustomEventsAsync(CancellationToken cancellationToken)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var query = dbContext.CustomEvents
            .Include(a => a.Teams)
            .OrderByDescending(a => a.StartDate)
            .AsQueryable();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CustomEvent>> GetCustomEventsAsync(string gameId, CancellationToken cancellationToken)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var query = dbContext.CustomEvents
            .Include(a => a.Teams)
            .Where(a => a.GameId == gameId)
            .OrderByDescending(a => a.StartDate)
            .AsQueryable();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent, CancellationToken cancellationToken)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var dbEvent = await dbContext.CustomEvents.AddAsync(customEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return dbEvent.Entity;
    }

    public async Task<CustomEvent> UpdateCustomEventAsync(CustomEvent customEvent, CancellationToken cancellationToken)
    {
        var updatedTeams = customEvent.Teams
            ?? throw new ArgumentException("Teams must be provided when updating a custom event.", nameof(customEvent));

        using var dbContext = _dbContextFactory.CreateDbContext();

        var oldCustomEvent = await dbContext.CustomEvents
            .AsNoTracking()
            .Include(a => a.Teams)
            .SingleOrDefaultAsync(a => a.Id == customEvent.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Custom event {customEvent.Id} does not exist.");

        var oldTeams = (oldCustomEvent.Teams ?? []).ToList();

        var addedTeams = updatedTeams.ExceptBy(oldTeams, a => a.TeamId).ToList();
        var deletedTeams = oldTeams.ExceptBy(updatedTeams, a => a.TeamId).ToList();

        deletedTeams.ForEach(a => dbContext.Entry(a).State = EntityState.Deleted);
        addedTeams.ForEach(a => dbContext.Entry(a).State = EntityState.Added);

        var dbEvent = dbContext.CustomEvents.Update(customEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return dbEvent.Entity;
    }

    public async Task RemoveCustomEventAsync(CustomEvent customEvent, CancellationToken cancellationToken)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        if (customEvent == null)
        {
            return;
        }

        dbContext.CustomEvents.Remove(customEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveCustomEventAsync(int customEventId, CancellationToken cancellationToken)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var customEvent = await dbContext.CustomEvents
            .Include(a => a.Teams)
            .SingleOrDefaultAsync(a => a.Id == customEventId, cancellationToken);

        if (customEvent == null)
        {
            return;
        }

        dbContext.CustomEvents.Remove(customEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
