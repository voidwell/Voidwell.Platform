using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.Repositories;

public interface ICustomEventRepository
{
    Task<CustomEvent?> GetCustomEventAsync(int customEventId);
    Task<IEnumerable<CustomEvent>> GetAllCustomEventsAsync(string? gameId = null, int skip = 0, int limit = -1);
    Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent);
    Task<CustomEvent> UpdateCustomEventAsync(CustomEvent customEvent);
    Task RemoveCustomEventAsync(int customEventId);
}
