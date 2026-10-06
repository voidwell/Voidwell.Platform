using System.ComponentModel.DataAnnotations;

namespace Voidwell.Platform.Clients.Keycloak;

public class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Keycloak:BaseUrl configuration value is required.")]
    public string BaseUrl { get; set; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Keycloak:Realm configuration value is required.")]
    public string Realm { get; set; } = null!;

    /// <summary>Service-account client used to call the Admin API. Needs the realm-management "view-users" role.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "The Keycloak:ClientId configuration value is required.")]
    public string ClientId { get; set; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Keycloak:ClientSecret configuration value is required.")]
    public string ClientSecret { get; set; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Keycloak:TokenServiceAddress configuration value is required.")]
    public string TokenServiceAddress { get; set; } = null!;

    public string Scopes { get; set; } = string.Empty;
}
