using System.ComponentModel.DataAnnotations;

namespace Voidwell.Platform.Data.Models;

public class BlogPostTag
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string NormalizedName { get; set; } = string.Empty;
}
