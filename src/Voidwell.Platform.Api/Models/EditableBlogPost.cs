namespace Voidwell.Platform.Api.Models;

public class EditableBlogPost
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MarkdownContent { get; set; } = string.Empty;
    public IEnumerable<BlogPostTag>? Tags { get; set; }
}
