using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Cache;
using Voidwell.Internal.Clients;
using Voidwell.Internal.Data;
using Voidwell.Internal.Data.Models;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
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

            var results = (from e in dbContext.CustomEvents
                                 orderby e.StartDate descending
                                 select new DbCustomEvent
                                 {
                                     Id = e.Id,
                                     Name = e.Name,
                                     Description = e.Description,
                                     GameId = e.GameId,
                                     IsPrivate = e.IsPrivate,
                                     StartDate = e.StartDate,
                                     EndDate = e.EndDate,
                                     ServerId = e.ServerId,
                                     MapId = e.MapId,
                                     Teams = (from t in dbContext.CustomEventTeams
                                              where t.EventId == e.Id
                                              select new DbCustomEventTeam
                                              {
                                                  EventId = t.EventId,
                                                  TeamId = t.TeamId,
                                                  Name = t.Name
                                              }).ToList()
                                 }).ToList();

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
                    GameId = r.GameId,
                    Teams = r.Teams?.Select(a => new CustomEventTeam { TeamId = a.TeamId, Name = a.Name })
                };
            });

            await _cache.SetAsync("customEventlist", customEvents, TimeSpan.FromMinutes(5));

            return customEvents;
        }

        public async Task<IEnumerable<CustomEvent>> GetAllCustomEvents(string gameId)
        {
            var cachedEvents = await _cache.GetAsync<IEnumerable<CustomEvent>>($"customEventlist_{gameId}");

            if (cachedEvents != null)
            {
                return cachedEvents;
            }

            var dbContext = _dbContextFactory();

            var results = (from e in dbContext.CustomEvents
                           where e.GameId == gameId && e.IsPrivate == false
                           orderby e.StartDate descending
                           select new DbCustomEvent
                           {
                               Id = e.Id,
                               Name = e.Name,
                               Description = e.Description,
                               GameId = e.GameId,
                               IsPrivate = e.IsPrivate,
                               StartDate = e.StartDate,
                               EndDate = e.EndDate,
                               ServerId = e.ServerId,
                               MapId = e.MapId,
                               Teams = (from t in dbContext.CustomEventTeams
                                        where t.EventId == e.Id
                                        select new DbCustomEventTeam
                                        {
                                            EventId = t.EventId,
                                            TeamId = t.TeamId,
                                            Name = t.Name
                                        }).ToList()
                           }).ToList();

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
                    GameId = r.GameId,
                    Teams = r.Teams?.Select(a => new CustomEventTeam { TeamId = a.TeamId, Name = a.Name })
                };
            });

            await _cache.SetAsync($"customEventlist_{gameId}", customEvents, TimeSpan.FromMinutes(5));

            return customEvents;
        }

        public async Task<CustomEventDetails> GetCustomEvent(int eventId)
        {
            var cacheKey = $"customEventLog:{eventId}";

            var cachedEvent = await _cache.GetAsync<CustomEventDetails>(cacheKey);
            if (cachedEvent != null)
            {
                return cachedEvent;
            }

            var dbContext = _dbContextFactory();

            var result = await GetCustomEventParams(eventId);

            var reportTask = _daybreakGamesClient.GetCombatReport(result.ServerId, result.MapId, result.StartDate, result.EndDate);
            var territoryTask = _daybreakGamesClient.GetTerritoryScoreFromDate(result.ServerId, result.MapId, result.EndDate);

            await Task.WhenAll(reportTask, territoryTask);

            var customEvent = new CustomEventDetails
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

            await _cache.SetAsync(cacheKey, customEvent, TimeSpan.FromMinutes(15));

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
                GameId = customEvent.GameId
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

            customEvent.Id = eventId;

            return customEvent;
        }

        public async Task<CustomEvent> UpdateCustomEvent(int eventId, CustomEvent customEvent)
        {
            var dbContext = _dbContextFactory();

            var dbEvent = (from e in dbContext.CustomEvents
                          where e.Id == eventId
                          select new DbCustomEvent
                          {
                              Id = e.Id,
                              Name = e.Name,
                              Description = e.Description,
                              GameId = e.GameId,
                              IsPrivate = e.IsPrivate,
                              StartDate = e.StartDate,
                              EndDate = e.EndDate,
                              ServerId = e.ServerId,
                              MapId = e.MapId,
                              Teams = (from t in dbContext.CustomEventTeams
                                       where t.EventId == e.Id
                                       select new DbCustomEventTeam
                                       {
                                           EventId = t.EventId,
                                           TeamId = t.TeamId,
                                           Name = t.Name
                                       }).ToList()
                          }).FirstOrDefault();

            if (dbEvent == null)
                return null;

            dbEvent.Name = customEvent.Name;
            dbEvent.ServerId = customEvent.ServerId;
            dbEvent.MapId = customEvent.MapId;
            dbEvent.Description = customEvent.Description;
            dbEvent.StartDate = customEvent.StartDate;
            dbEvent.EndDate = customEvent.EndDate;
            dbEvent.IsPrivate = customEvent.IsPrivate;

            dbContext.Update(dbEvent);
            await dbContext.SaveChangesAsync();

            dbContext.CustomEventTeams.RemoveRange(dbEvent.Teams);
            await dbContext.SaveChangesAsync();

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

            return customEvent;
        }

        public async Task DeleteCustomEvent(int eventId)
        {
            var dbContext = _dbContextFactory();

            var dbEvent = await dbContext.CustomEvents
                .SingleOrDefaultAsync(e => e.Id == eventId);

            dbContext.CustomEvents.Remove(dbEvent);
            await dbContext.SaveChangesAsync();
        }

        private async Task<CustomEvent> GetCustomEventParams(int eventId)
        {
            var dbContext = _dbContextFactory();

            var result = (from e in dbContext.CustomEvents
                           where e.Id == eventId
                           select new DbCustomEvent
                           {
                               Id = e.Id,
                               Name = e.Name,
                               Description = e.Description,
                               GameId = e.GameId,
                               IsPrivate = e.IsPrivate,
                               StartDate = e.StartDate,
                               EndDate = e.EndDate,
                               ServerId = e.ServerId,
                               MapId = e.MapId,
                               Teams = (from t in dbContext.CustomEventTeams
                                        where t.EventId == e.Id
                                        select new DbCustomEventTeam
                                        {
                                            EventId = t.EventId,
                                            TeamId = t.TeamId,
                                            Name = t.Name
                                        }).AsNoTracking().ToList()
                           }).AsNoTracking().FirstOrDefault();

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
                Teams = result.Teams?.Select(a => new CustomEventTeam { TeamId = a.TeamId, Name = a.Name })
            };

            return await Task.FromResult(customEvent);
        }
    }
}
