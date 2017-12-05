using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Internal.Clients;
using Voidwell.Internal.Data;
using Voidwell.Internal.Data.Models;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public class BlogService : IBlogService
    {
        private readonly Func<VoidwellDbContext> _dbContextFactory;
        private readonly IUserManagementClient _userManagementClient;

        public BlogService(Func<VoidwellDbContext> dbContextFactory, IUserManagementClient userManagementClient)
        {
            _dbContextFactory = dbContextFactory;
            _userManagementClient = userManagementClient;
        }

        public async Task<BlogPost> CreateBlogPost(Guid authorId, BlogPost blogPost)
        {
            var dbContext = _dbContextFactory();

            var dbPost = await dbContext.BlogPosts.AddAsync(new DbBlogPost
            {
                Title = blogPost.Title,
                PublishDate = DateTime.UtcNow,
                AuthorId = authorId,
                Content = blogPost.Content
            });
            await dbContext.SaveChangesAsync();

            blogPost.Id = dbPost.Entity.Id;

            return blogPost;
        }

        public async Task DeleteBlogPost(string postId)
        {
            var dbContext = _dbContextFactory();

            var dbPost = await dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == postId);

            if (dbPost != null)
            {
                dbContext.BlogPosts.Remove(dbPost);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<BlogPost> GetBlogPost(string postId)
        {
            var dbContext = _dbContextFactory();

            var dbPost = await dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == postId);

            if (dbPost == null)
                return null;

            var author = await _userManagementClient.GetDisplayName(dbPost.AuthorId);

            return new BlogPost
            {
                Id = dbPost.Id,
                Title = dbPost.Title,
                PublishDate = dbPost.PublishDate,
                Author = author.Name,
                Content = dbPost.Content
            };
        }

        public async Task<IEnumerable<BlogPost>> GetBlogPosts()
        {
            var dbContext = _dbContextFactory();

            var dbPosts = await dbContext.BlogPosts.Take(10)
                .OrderBy("PublishDate", SortDirection.Descending)
                .ToListAsync();

            var work = dbPosts.Select(async p =>
            {
                var author = await _userManagementClient.GetDisplayName(p.AuthorId);

                return new BlogPost
                {
                    Id = p.Id,
                    Title = p.Title,
                    Author = author.Name,
                    PublishDate = p.PublishDate,
                    Content = p.Content
                };
            });

            await Task.WhenAll(work);

            return work.Select(a => a.Result);
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
