using System.ComponentModel.DataAnnotations;

namespace Voidwell.Platform.Clients.AuthenticatedHttpClient;

public class AuthenticatedHttpClientOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "The TokenServiceAddress configuration value is required.")]
    public string TokenServiceAddress { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The ClientId configuration value is required.")]
    public string ClientId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The ClientSecret configuration value is required.")]
    public string ClientSecret { get; set; } = string.Empty;

    public List<string> ClientScopes { get; set; } = [];
}
