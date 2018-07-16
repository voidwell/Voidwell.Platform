using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Internal.Data.Models;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public interface ICustomEventService
    {
        Task<IEnumerable<CustomEvent>> GetAllCustomEventsAsync();
        Task<IEnumerable<CustomEvent>> GetAllCustomEventsByGameIdAsync(string gameId);
        Task<CustomEventDetails> GetCustomEventAsync(int eventId);
        Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent);
        Task<CustomEvent> UpdateCustomEventAsync(int eventId, CustomEvent customEvent);
        Task DeleteCustomEventAsync(int eventId);
    }
}
