using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public interface ICustomEventService
    {
        Task<IEnumerable<CustomEvent>> GetAllCustomEvents();
        Task<CustomEvent> GetCustomEvent(string eventId);
        Task<CustomEvent> CreateCustomEvent(CustomEvent customEvent);
        Task<CustomEvent> UpdateCustomEvent(string eventId, CustomEvent customEvent);
        Task DeleteCustomEvent(string eventId);
    }
}
