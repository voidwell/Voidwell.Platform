using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Api.Services;

public interface ICustomEventService
{
    Task<IEnumerable<CustomEvent>> GetAllCustomEventsAsync();
    Task<IEnumerable<CustomEvent>> GetAllCustomEventsByGameIdAsync(string gameId);
    Task<CustomEventDetails?> GetCustomEventAsync(int eventId);
    Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent);
    Task<CustomEvent> UpdateCustomEventAsync(int eventId, CustomEvent customEvent);
    Task DeleteCustomEventAsync(int eventId);
}
