using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Voidwell.Platform.Clients.AuthenticatedHttpClient;

internal sealed class ClientTokenService : IClientTokenService
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<AuthenticatedHttpClientOptions> _optionsMonitor;

    public ClientTokenService(HttpClient httpClient, IOptionsMonitor<AuthenticatedHttpClientOptions> optionsMonitor)
    {
        _httpClient = httpClient;
        _optionsMonitor = optionsMonitor;
    }

    public async Task<TokenResponse> RequestTokenAsync<TClient>(CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.Get(typeof(TClient).Name);
        using var request = new HttpRequestMessage(HttpMethod.Post, options.TokenServiceAddress)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = options.ClientId,
                ["client_secret"] = options.ClientSecret,
                ["scope"] = string.Join(' ', options.ClientScopes)
            })
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        if (token is null || string.IsNullOrEmpty(token.AccessToken))
        {
            throw new InvalidOperationException("The token service returned no access token.");
        }

        return token;
    }
}

internal sealed class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
