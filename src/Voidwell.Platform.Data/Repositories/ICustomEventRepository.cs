using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.Repositories;

public interface ICustomEventRepository
{
    Task<CustomEvent?> GetCustomEventAsync(int customEventId, CancellationToken cancellationToken);
    Task<IEnumerable<CustomEvent>> GetCustomEventsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<CustomEvent>> GetCustomEventsAsync(string gameId, CancellationToken cancellationToken);
    Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent, CancellationToken cancellationToken);
    Task<CustomEvent> UpdateCustomEventAsync(CustomEvent customEvent, CancellationToken cancellationToken);
    Task RemoveCustomEventAsync(CustomEvent customEvent, CancellationToken cancellationToken);
    Task RemoveCustomEventAsync(int customEventId, CancellationToken cancellationToken);
}
