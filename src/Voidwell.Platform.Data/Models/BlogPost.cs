using System.ComponentModel.DataAnnotations;

namespace Voidwell.Platform.Data.Models;

public class BlogPost
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public Guid AuthorId { get; set; }
    [Required]
    public DateTimeOffset PublishDate { get; set; }
    [Required]
    public string MarkdownContent { get; set; } = string.Empty;
    [Required]
    public string HtmlContent { get; set; } = string.Empty;

    public IEnumerable<BlogPostTagMap>? BlogPostTagMaps { get; set; }
}
