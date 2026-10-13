using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Api.Services;

public interface ICustomEventService
{
    Task<IEnumerable<CustomEvent>> GetCustomEventsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<CustomEvent>> GetCustomEventsAsync(string gameId, CancellationToken cancellationToken);
    Task<CustomEventDetails?> GetCustomEventAsync(int eventId, CancellationToken cancellationToken);
    Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent, CancellationToken cancellationToken);
    Task<CustomEvent> UpdateCustomEventAsync(int eventId, CustomEvent customEvent, CancellationToken cancellationToken);
    Task DeleteCustomEventAsync(int eventId, CancellationToken cancellationToken);
}
