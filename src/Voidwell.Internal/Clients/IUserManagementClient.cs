using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Clients
{
    public interface IUserManagementClient
    {
        Task<DisplayName> GetDisplayName(Guid userId);
        Task<IEnumerable<DisplayName>> GetDisplayNames(IEnumerable<Guid> userIds);
    }
}
