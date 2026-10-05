using System.ComponentModel.DataAnnotations;

namespace Voidwell.Platform.Api.Models;

public class BlogPostRequest
{
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string MarkdownContent { get; set; } = string.Empty;
    public IEnumerable<BlogPostTag>? Tags { get; set; }
}
