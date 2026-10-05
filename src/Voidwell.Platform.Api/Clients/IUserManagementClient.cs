using Voidwell.Platform.Api.Models;

namespace Voidwell.Platform.Api.Clients;

public interface IUserManagementClient
{
    Task<DisplayName?> GetDisplayNameAsync(Guid userId);
    Task<IEnumerable<DisplayName>?> GetDisplayNamesAsync(IEnumerable<Guid> userIds);
}
