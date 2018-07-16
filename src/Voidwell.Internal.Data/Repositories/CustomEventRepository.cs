using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Internal.Data.Models;

namespace Voidwell.Internal.Data.Repositories
{
    public class CustomEventRepository : ICustomEventRepository
    {
        private readonly IDbContextHelper _dbContextHelper;

        public CustomEventRepository(IDbContextHelper dbContextHelper)
        {
            _dbContextHelper = dbContextHelper;
        }

        public async Task<CustomEvent> GetCustomEventAsync(int customEventId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.CustomEvents
                    .Include(a => a.Teams)
                    .SingleOrDefaultAsync(a => a.Id == customEventId);
            }
        }

        public async Task<IEnumerable<CustomEvent>> GetAllCustomEventsAsync(string gameId = null, int skip = 0, int limit = -1)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

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
        }

        public async Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var dbEvent = await dbContext.CustomEvents.AddAsync(customEvent);
                await dbContext.SaveChangesAsync();

                return dbEvent.Entity;
            }
        }

        public async Task<CustomEvent> UpdateCustomEventAsync(CustomEvent customEvent)
        {
            var updatedTeams = customEvent.Teams;

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var oldCustomEvent = await GetCustomEventAsync(customEvent.Id);
                List<CustomEventTeam> oldTeams = oldCustomEvent.Teams.ToList();

                List<CustomEventTeam> addedTeams = updatedTeams.ExceptBy(oldTeams, a => a.TeamId).ToList();
                List<CustomEventTeam> deletedTeams = oldTeams.ExceptBy(updatedTeams, a => a.TeamId).ToList();

                deletedTeams.ForEach(a => dbContext.Entry(a).State = EntityState.Deleted);
                addedTeams.ForEach(a => dbContext.Entry(a).State = EntityState.Added);

                var dbEvent = dbContext.CustomEvents.Update(customEvent);
                await dbContext.SaveChangesAsync();

                return dbEvent.Entity;
            }
        }

        public async Task RemoveCustomEventAsync(int customEventId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var customEvent = await GetCustomEventAsync(customEventId);
                if (customEvent == null)
                {
                    return;
                }

                dbContext.CustomEvents.Remove(customEvent);
            }
        }
    }
}
