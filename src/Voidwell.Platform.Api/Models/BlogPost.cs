namespace Voidwell.Platform.Api.Models;

public class BlogPost
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? AuthorName { get; set; }
    public DateTimeOffset PublishDate { get; set; }
    public IEnumerable<BlogPostTag>? Tags { get; set; }
}
