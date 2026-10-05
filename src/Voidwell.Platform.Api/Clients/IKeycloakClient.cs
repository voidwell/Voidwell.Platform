using Voidwell.Platform.Api.Models;

namespace Voidwell.Platform.Api.Clients;

public interface IKeycloakClient
{
    /// <summary>Returns the display name for a user, or null when the user does not exist.</summary>
    Task<DisplayName?> GetDisplayNameAsync(Guid userId);

    /// <summary>Returns display names for the users that exist; unknown users are omitted.</summary>
    Task<IEnumerable<DisplayName>> GetDisplayNamesAsync(IEnumerable<Guid> userIds);
}
