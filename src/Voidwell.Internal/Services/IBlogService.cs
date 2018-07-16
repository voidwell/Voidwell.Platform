using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Internal.Data.Models;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public interface IBlogService
    {
        Task<BlogPostModel> GetBlogPostAsync(string postId);
        Task<IEnumerable<BlogPostModel>> GetBlogPostsAsync(int page, int limit);
        Task<BlogPost> CreateBlogPostAsync(Guid authorId, BlogPost blogPost);
        Task<BlogPost> UpdateBlogPostAsync(string postId, BlogPost blogPost);
        Task DeleteBlogPostAsync(string postId);
    }
}