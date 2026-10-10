namespace Voidwell.Platform.Clients.Keycloak.Models;

public class KeycloakUser
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public Dictionary<string, List<string>>? Attributes { get; set; }
}
