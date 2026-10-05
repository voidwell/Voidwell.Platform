namespace Voidwell.Platform.Api.Options;

public class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>Root address of the Keycloak server, e.g. https://auth.voidwell.com.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Realm that holds the Voidwell users.</summary>
    public string? Realm { get; set; }

    /// <summary>Service-account client used to call the Admin API. Needs the realm-management "view-users" role.</summary>
    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }
}
