using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Internal.Data.Models;

namespace Voidwell.Internal.Data.Repositories
{
    public interface IBlogPostRepository
    {
        Task<BlogPost> GetBlogPostAsync(string blogPostId);
        Task<IEnumerable<BlogPost>> GetAllBlogPostsAsync(int skip = 0, int limit = -1);
        Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost);
        Task<BlogPost> UpdateBlogPostAsync(BlogPost blogPost);
        Task RemoveBlogPostAsync(string blogPostId);
    }
}