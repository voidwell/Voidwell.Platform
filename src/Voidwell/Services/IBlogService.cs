using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Models;

namespace Voidwell.Services
{
    public interface IBlogService
    {
        Task<BlogPost> GetBlogPost(string postId);
        Task<IEnumerable<BlogPost>> GetBlogPosts();
        Task<BlogPost> CreateBlogPost(BlogPost blogPost);
        Task<BlogPost> UpdateBlogPost(string postId, BlogPost blogPost);
        Task DeleteBlogPost(string postId);
    }
}