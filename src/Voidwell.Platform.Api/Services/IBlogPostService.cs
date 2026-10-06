using Voidwell.Platform.Api.Models;

namespace Voidwell.Platform.Api.Services;

public interface IBlogPostService
{
    Task<IEnumerable<BlogPost>> GetBlogPostsByPageAsync(int page);
    Task<BlogPost?> GetBlogPostByIdAsync(Guid blogPostId);
    Task<BlogPost> CreateBlogPostAsync(Guid authorId, BlogPostRequest blogPostRequest);
    Task<BlogPost?> UpdateBlogPostAsync(Guid blogPostId, BlogPostRequest blogPostRequest);
    Task DeleteBlogPostAsync(Guid blogPostId);
    Task<EditableBlogPost?> GetEditableBlogPostByIdAsync(Guid blogPostId);
}
