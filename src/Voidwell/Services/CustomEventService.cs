using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Cache;
using Voidwell.Clients;
using Voidwell.Data.DBContext;
using Voidwell.Data.Models;
using Voidwell.Models;

namespace Voidwell.Services
{
    public class CustomEventService : ICustomEventService
    {
        private readonly Func<VoidwellDbContext> _dbContextFactory;
        private readonly ICache _cache;
        private readonly IDaybreakGamesClient _daybreakGamesClient;

        public CustomEventService(Func<VoidwellDbContext> dbContextFactory, ICache cache, IDaybreakGamesClient daybreakGamesClient)
        {
            _dbContextFactory = dbContextFactory;
            _cache = cache;
            _daybreakGamesClient = daybreakGamesClient;
        }

        public async Task<IEnumerable<CustomEvent>> GetAllCustomEvents()
        {
            var cachedEvents = await _cache.GetAsync<IEnumerable<CustomEvent>>("customEventlist");

            if (cachedEvents != null)
                return cachedEvents;

            var dbContext = _dbContextFactory();

            var results = await dbContext.CustomEvents.Where(e => e.IsPrivate == false)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            var customEvents = results.Select(r =>
            {
                return new CustomEvent
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    ServerId = r.ServerId,
                    MapId = r.MapId,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    IsPrivate = r.IsPrivate,
                    GameId = r.GameId
                };
            });

            await _cache.SetAsync("customEventlist", customEvents, TimeSpan.FromMinutes(30));

            return customEvents;
        }

        public async Task<CustomEvent> GetCustomEvent(string eventId)
        {
            var cacheKey = $"customEventLog:{eventId}";

            var cachedEvent = await _cache.GetAsync<CustomEvent>(cacheKey);

            if (cachedEvent != null)
                return cachedEvent;

            var dbContext = _dbContextFactory();

            var result = await dbContext.CustomEvents
                .Include(a => a.Teams)
                .SingleOrDefaultAsync(e => e.Id == eventId);

            var reportTask = _daybreakGamesClient.GetCombatReport(result.ServerId, result.MapId, result.StartDate, result.EndDate);
            var territoryTask = _daybreakGamesClient.GetTerritoryScoreFromDate(result.ServerId, result.MapId, result.EndDate);

            await Task.WhenAll(reportTask, territoryTask);

            var customEvent = new CustomEvent
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                ServerId = result.ServerId,
                MapId = result.MapId,
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                IsPrivate = result.IsPrivate,
                GameId = result.GameId,
                Teams = result.Teams.Select(t => new CustomEventTeam { TeamId = t.TeamId, Name = t.Name }),
                Log = reportTask.Result,
                Score = territoryTask.Result
            };

            await _cache.SetAsync(cacheKey, customEvent, TimeSpan.FromMinutes(60));

            return customEvent;
        }

        public async Task<CustomEvent> CreateCustomEvent(CustomEvent customEvent)
        {
            var dbCustomEventModel = new DbCustomEvent
            {
                Name = customEvent.Name,
                ServerId = customEvent.ServerId,
                MapId = customEvent.MapId,
                Description = customEvent.Description,
                StartDate = customEvent.StartDate,
                EndDate = customEvent.EndDate,
                IsPrivate = customEvent.IsPrivate,
                GameId = "ps2"
            };

            var dbContext = _dbContextFactory();

            var dbCustomEvent = await dbContext.CustomEvents.AddAsync(dbCustomEventModel);
            var eventId = dbCustomEvent.Entity.Id;

            var dbCustomEventTeamModels = customEvent.Teams.Select(t =>
            {
                return new DbCustomEventTeam
                {
                    EventId = eventId,
                    TeamId = t.TeamId,
                    Name = t.Name
                };
            });

            await dbContext.CustomEventTeams.AddRangeAsync(dbCustomEventTeamModels);

            await dbContext.SaveChangesAsync();

            return await GetCustomEvent(eventId);
        }

        public async Task<CustomEvent> UpdateCustomEvent(string eventId, CustomEvent customEvent)
        {
            var dbContext = _dbContextFactory();

            var dbEvent = await dbContext.CustomEvents
                .Include(i => i.Teams)
                .SingleOrDefaultAsync(e => e.Id == eventId);

            if (dbEvent == null)
                return null;

            dbEvent.Name = customEvent.Name;
            dbEvent.ServerId = customEvent.ServerId;
            dbEvent.MapId = customEvent.MapId;
            dbEvent.Description = customEvent.Description;
            dbEvent.StartDate = customEvent.StartDate;
            dbEvent.EndDate = customEvent.EndDate;
            dbEvent.IsPrivate = customEvent.IsPrivate;

            dbContext.CustomEventTeams.RemoveRange(dbEvent.Teams);

            var dbCustomEventTeamModels = customEvent.Teams.Select(t =>
            {
                return new DbCustomEventTeam
                {
                    EventId = eventId,
                    TeamId = t.TeamId,
                    Name = t.Name
                };
            });

            await dbContext.CustomEventTeams.AddRangeAsync(dbCustomEventTeamModels);

            await dbContext.SaveChangesAsync();

            return await GetCustomEvent(eventId);
        }

        public async Task DeleteCustomEvent(string eventId)
        {
            var dbContext = _dbContextFactory();

            var dbEvent = await dbContext.CustomEvents
                .SingleOrDefaultAsync(e => e.Id == eventId);

            dbContext.CustomEvents.Remove(dbEvent);
            await dbContext.SaveChangesAsync();
        }
    }
}
