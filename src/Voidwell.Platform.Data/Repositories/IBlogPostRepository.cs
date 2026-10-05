using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.Repositories;

public interface IBlogPostRepository
{
    Task<IEnumerable<BlogPost>> GetBlogPostsAsync(int page, int limit);
    Task<BlogPost?> GetBlogPostAsync(Guid blogPostId);
    Task<Guid> CreateBlogPostAsync(BlogPost blogPost);
    Task UpdateBlogPostAsync(BlogPost blogPost);
    Task DeleteBlogPostAsync(Guid blogPostId);
}
