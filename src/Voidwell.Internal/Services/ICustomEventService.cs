using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public interface ICustomEventService
    {
        Task<IEnumerable<CustomEvent>> GetAllCustomEvents();
        Task<IEnumerable<CustomEvent>> GetAllCustomEvents(string gameId);
        Task<CustomEventDetails> GetCustomEvent(int eventId);
        Task<CustomEvent> CreateCustomEvent(CustomEvent customEvent);
        Task<CustomEvent> UpdateCustomEvent(int eventId, CustomEvent customEvent);
        Task DeleteCustomEvent(int eventId);
    }
}
