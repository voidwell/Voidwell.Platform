using System;
using System.Threading.Tasks;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Clients
{
    public interface IUserManagementClient
    {
        Task<DisplayName> GetDisplayName(Guid userId);
    }
}
