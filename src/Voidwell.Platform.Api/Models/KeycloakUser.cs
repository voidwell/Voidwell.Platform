using System.Text.Json.Serialization;

namespace Voidwell.Platform.Api.Models;

public class KeycloakUser
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
}

public class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
