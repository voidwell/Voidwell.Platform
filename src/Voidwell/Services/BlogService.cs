using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Data.DBContext;
using Voidwell.Data.Models;
using Voidwell.Models;

namespace Voidwell.Services
{
    public class BlogService : IBlogService
    {
        private readonly Func<VoidwellDbContext> _dbContextFactory;

        public BlogService(Func<VoidwellDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<BlogPost> CreateBlogPost(BlogPost blogPost)
        {
            var dbContext = _dbContextFactory();

            var dbPost = await dbContext.BlogPosts.AddAsync(new DbBlogPost
            {
                Title = blogPost.Title,
                PublishDate = DateTime.UtcNow,
                Author = blogPost.Author,
                Content = blogPost.Content
            });
            await dbContext.SaveChangesAsync();

            blogPost.Id = dbPost.Entity.Id;

            return blogPost;
        }

        public async Task DeleteBlogPost(string postId)
        {
            var dbContext = _dbContextFactory();

            var dbPost = await dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == postId && a.IsDeleted == false);

            if (dbPost != null)
            {
                dbPost.IsDeleted = true;
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<BlogPost> GetBlogPost(string postId)
        {
            var dbContext = _dbContextFactory();

            var dbPost = await dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == postId && a.IsDeleted == false);

            if (dbPost == null)
                return null;

            return new BlogPost
            {
                Id = dbPost.Id,
                Title = dbPost.Title,
                PublishDate = dbPost.PublishDate,
                Author = dbPost.Author,
                Content = dbPost.Content
            };
        }

        public async Task<IEnumerable<BlogPost>> GetBlogPosts()
        {
            var dbContext = _dbContextFactory();

            var dbPosts = await dbContext.BlogPosts.Where(a => a.IsDeleted == false)
                .Take(10)
                .ToListAsync();

            return dbPosts.Select(p =>
            {
                return new BlogPost
                {
                    Id = p.Id,
                    Title = p.Title,
                    PublishDate = p.PublishDate,
                    Author = p.Author,
                    Content = p.Content
                };
            });
        }

        public async Task<BlogPost> UpdateBlogPost(string postId, BlogPost blogPost)
        {
            var dbContext = _dbContextFactory();

            var dbPost = await dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == postId);

            dbPost.Title = blogPost.Title;
            dbPost.Content = blogPost.Content;

            await dbContext.SaveChangesAsync();

            return blogPost;
        }
    }
}
