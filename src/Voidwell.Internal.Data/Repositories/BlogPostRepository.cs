using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Internal.Data.Models;

namespace Voidwell.Internal.Data.Repositories
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly IDbContextHelper _dbContextHelper;

        public BlogPostRepository(IDbContextHelper dbContextHelper)
        {
            _dbContextHelper = dbContextHelper;
        }

        public async Task<BlogPost> GetBlogPostAsync(string blogPostId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == blogPostId);
            }
        }

        public async Task<IEnumerable<BlogPost>> GetAllBlogPostsAsync(int skip = 0, int limit = -1)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var query = dbContext.BlogPosts
                    .Take(skip)
                    .OrderByDescending(a => a.PublishDate)
                    .AsQueryable();

                if (limit > 0)
                {
                    query = query.Take(limit);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var dbPost = await dbContext.BlogPosts.AddAsync(blogPost);
                await dbContext.SaveChangesAsync();

                return dbPost.Entity;
            }
        }

        public async Task<BlogPost> UpdateBlogPostAsync(BlogPost blogPost)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var post = await GetBlogPostAsync(blogPost.Id);
                if (post == null)
                {
                    return null;
                }

                post.Title = blogPost.Title;
                post.Content = blogPost.Content;

                dbContext.BlogPosts.Update(post);
                await dbContext.SaveChangesAsync();

                return post;
            }
        }

        public async Task RemoveBlogPostAsync(string blogPostId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var blogPost = await GetBlogPostAsync(blogPostId);
                if (blogPost == null)
                {
                    return;
                }

                dbContext.BlogPosts.Remove(blogPost);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
