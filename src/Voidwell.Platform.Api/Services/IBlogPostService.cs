using Voidwell.Platform.Api.Models;

namespace Voidwell.Platform.Api.Services;

public interface IBlogPostService
{
    Task<IEnumerable<BlogPost>> GetBlogPostsByPageAsync(int page);
    Task<BlogPost?> GetBlogPostByIdAsync(Guid blogPostId);
    Task<BlogPost> CreateBlogPostAsync(Guid authorId, BlogPostRequest blogPostRequest);
    Task<BlogPost?> UpdateBlogPostAsync(BlogPostRequest blogPostRequest);
    Task DeleteBlogPostAsync(Guid blogPostId);
    Task<BlogPostRequest?> GetEditableBlogPostByIdAsync(Guid blogPostId);
}
