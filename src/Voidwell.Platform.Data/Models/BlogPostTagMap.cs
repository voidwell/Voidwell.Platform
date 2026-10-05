using System.ComponentModel.DataAnnotations;

namespace Voidwell.Platform.Data.Models;

public class BlogPostTagMap
{
    [Required]
    public Guid BlogPostId { get; set; }
    [Required]
    public Guid BlogPostTagId { get; set; }

    public BlogPost? BlogPost { get; set; }
    public BlogPostTag? BlogPostTag { get; set; }
}
