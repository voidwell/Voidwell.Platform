using System.Net;
using Microsoft.Extensions.Options;
using Voidwell.Platform.Clients.Keycloak.Models;

namespace Voidwell.Platform.Clients.Keycloak;

/// <summary>
/// Resolves user display names through the Keycloak Admin REST API using a client-credentials service account.
/// </summary>
public class KeycloakClient : IKeycloakClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;

    public KeycloakClient(HttpClient httpClient, IOptions<KeycloakOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<DisplayName?> GetDisplayNameAsync(Guid userId)
    {
        using var response = await _httpClient.GetAsync($"admin/realms/{_options.Realm}/users/{userId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var user = await response.GetContentAsync<KeycloakUser>();
        if (user == null)
        {
            return null;
        }

        return new DisplayName { UserId = user.Id, Name = user.Username };
    }

    public async Task<IEnumerable<DisplayName>> GetDisplayNamesAsync(IEnumerable<Guid> userIds)
    {
        // Keycloak has no batch lookup by id, so resolve each distinct user individually
        var lookups = userIds.Distinct().Select(GetDisplayNameAsync);
        var displayNames = await Task.WhenAll(lookups);

        return displayNames.OfType<DisplayName>().ToList();
    }
}
