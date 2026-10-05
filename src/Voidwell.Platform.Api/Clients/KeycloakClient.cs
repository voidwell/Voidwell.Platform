using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Api.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Voidwell.Platform.Api.Clients;

/// <summary>
/// Resolves user display names through the Keycloak Admin REST API using a client-credentials service account.
/// </summary>
public class KeycloakClient : IKeycloakClient
{
    private const string _tokenCacheKey = "keycloak_admin_token";
    private static readonly TimeSpan _tokenExpiryMargin = TimeSpan.FromSeconds(30);

    private readonly HttpClient _httpClient;
    private readonly IFusionCache _cache;
    private readonly KeycloakOptions _options;

    public KeycloakClient(HttpClient httpClient, IFusionCache cache, IOptions<KeycloakOptions> options)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<DisplayName?> GetDisplayNameAsync(Guid userId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri($"admin/realms/{Realm}/users/{userId}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync());

        using var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var user = await response.GetContentAsync<KeycloakUser>();
        if (user == null)
        {
            return null;
        }

        return new DisplayName { UserId = user.Id, Name = user.Username ?? string.Empty };
    }

    public async Task<IEnumerable<DisplayName>> GetDisplayNamesAsync(IEnumerable<Guid> userIds)
    {
        // Keycloak has no batch lookup by id, so resolve each distinct user individually
        var lookups = userIds.Distinct().Select(GetDisplayNameAsync);
        var displayNames = await Task.WhenAll(lookups);

        return displayNames.OfType<DisplayName>().ToList();
    }

    private string Realm => Required(_options.Realm, nameof(KeycloakOptions.Realm));

    private Uri BuildUri(string path)
    {
        var baseUrl = Required(_options.BaseUrl, nameof(KeycloakOptions.BaseUrl));
        return new Uri($"{baseUrl.TrimEnd('/')}/{path}");
    }

    private async Task<string> GetAccessTokenAsync()
    {
        return await _cache.GetOrSetAsync<string>(
            _tokenCacheKey,
            async (context, cancellationToken) =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri($"realms/{Realm}/protocol/openid-connect/token"))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials",
                        ["client_id"] = Required(_options.ClientId, nameof(KeycloakOptions.ClientId)),
                        ["client_secret"] = Required(_options.ClientSecret, nameof(KeycloakOptions.ClientSecret))
                    })
                };

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var token = await response.GetContentAsync<KeycloakTokenResponse>();
                if (string.IsNullOrEmpty(token?.AccessToken))
                {
                    throw new InvalidOperationException("Keycloak did not return an access token.");
                }

                // Refresh slightly before the token expires
                var lifetime = TimeSpan.FromSeconds(token.ExpiresIn) - _tokenExpiryMargin;
                context.Options.Duration = lifetime > TimeSpan.Zero ? lifetime : TimeSpan.Zero;

                return token.AccessToken;
            },
            new FusionCacheEntryOptions
            {
                // The service-account token must never be shared through Redis
                SkipDistributedCacheRead = true,
                SkipDistributedCacheWrite = true,
                SkipBackplaneNotifications = true
            });
    }

    private static string Required(string? value, string name)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{KeycloakOptions.SectionName}:{name} is not configured.")
            : value;
    }
}
