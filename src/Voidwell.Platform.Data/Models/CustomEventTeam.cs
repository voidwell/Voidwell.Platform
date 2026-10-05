using System.ComponentModel.DataAnnotations;

namespace Voidwell.Platform.Data.Models;

public class CustomEventTeam
{
    [Required]
    public int CustomEventId { get; set; }
    [Required]
    public string TeamId { get; set; } = string.Empty;
    [Required]
    public string Name { get; set; } = string.Empty;

    public CustomEvent? CustomEvent { get; set; }
}
