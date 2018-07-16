using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Internal.Data.Models;

namespace Voidwell.Internal.Data.Repositories
{
    public interface ICustomEventRepository
    {
        Task<CustomEvent> GetCustomEventAsync(int customEventId);
        Task<IEnumerable<CustomEvent>> GetAllCustomEventsAsync(string gameId = null, int skip = 0, int limit = -1);
        Task<CustomEvent> CreateCustomEventAsync(CustomEvent customEvent);
        Task<CustomEvent> UpdateCustomEventAsync(CustomEvent customEvent);
        Task RemoveCustomEventAsync(int customEventId);
    }
}