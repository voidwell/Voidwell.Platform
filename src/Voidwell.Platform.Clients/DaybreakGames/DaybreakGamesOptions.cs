using System.ComponentModel.DataAnnotations;

namespace Voidwell.Platform.Clients.DaybreakGames;

public class DaybreakGamesOptions
{
    public const string SectionName = "DaybreakGames";

    [Required(AllowEmptyStrings = false, ErrorMessage = "The DaybreakGames:BaseUrl configuration value is required.")]
    public string BaseUrl { get; set; } = "https://api.voidwell.com/ps2";

    [Required(AllowEmptyStrings = false, ErrorMessage = "The DaybreakGames:ClientId configuration value is required.")]
    public string ClientId { get; set; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The DaybreakGames:ClientSecret configuration value is required.")]
    public string ClientSecret { get; set; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The DaybreakGames:TokenServiceAddress configuration value is required.")]
    public string TokenServiceAddress { get; set; } = null!;

    public string Scopes { get; set; } = string.Empty;
}
