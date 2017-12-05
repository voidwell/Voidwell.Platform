using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public interface IBlogService
    {
        Task<BlogPost> GetBlogPost(string postId);
        Task<IEnumerable<BlogPost>> GetBlogPosts();
        Task<BlogPost> CreateBlogPost(Guid authorId, BlogPost blogPost);
        Task<BlogPost> UpdateBlogPost(string postId, BlogPost blogPost);
        Task DeleteBlogPost(string postId);
    }
}