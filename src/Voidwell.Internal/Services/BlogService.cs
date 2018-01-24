using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Cache;
using Voidwell.Internal.Clients;
using Voidwell.Internal.Data;
using Voidwell.Internal.Data.Models;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public class BlogService : IBlogService, IDisposable
    {
        private readonly VoidwellDbContext _dbContext;
        private readonly IUserManagementClient _userManagementClient;
        private readonly ICache _cache;

        private readonly string _cacheKey = "blog";
        private readonly TimeSpan _blogPostListCacheExpiration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _blogPostCacheExpiration = TimeSpan.FromMinutes(10);

        public BlogService(VoidwellDbContext dbContext, IUserManagementClient userManagementClient, ICache cache)
        {
            _dbContext = dbContext;
            _userManagementClient = userManagementClient;
            _cache = cache;
        }

        public async Task<BlogPost> CreateBlogPost(Guid authorId, BlogPost blogPost)
        {
            var dbPost = await _dbContext.BlogPosts.AddAsync(new DbBlogPost
            {
                Title = blogPost.Title,
                PublishDate = DateTime.UtcNow,
                AuthorId = authorId,
                Content = blogPost.Content
            });
            await _dbContext.SaveChangesAsync();

            blogPost.Id = dbPost.Entity.Id;

            return blogPost;
        }

        public async Task DeleteBlogPost(string postId)
        {
            var dbPost = await _dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == postId);

            if (dbPost != null)
            {
                _dbContext.BlogPosts.Remove(dbPost);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<BlogPost> GetBlogPost(string postId)
        {
            var cacheKey = $"{_cacheKey}_post_{postId}";

            var blogPost = await _cache.GetAsync<BlogPost>(cacheKey);
            if (blogPost != null)
            {
                return blogPost;
            }

            var dbPost = await _dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == postId);

            if (dbPost == null)
                return null;

            var author = await _userManagementClient.GetDisplayName(dbPost.AuthorId);

            blogPost =  new BlogPost
            {
                Id = dbPost.Id,
                Title = dbPost.Title,
                PublishDate = dbPost.PublishDate,
                Author = author.Name,
                Content = dbPost.Content
            };

            if (blogPost != null)
            {
                await _cache.SetAsync(cacheKey, blogPost, _blogPostCacheExpiration);
            }

            return blogPost;
        }

        public async Task<IEnumerable<BlogPost>> GetBlogPosts(int limit, int page)
        {
            var cacheKey = $"{_cacheKey}_postlist_{limit}_{page}";

            var blogPosts = await _cache.GetAsync<IEnumerable<BlogPost>>(cacheKey);
            if (blogPosts != null)
            {
                return blogPosts;
            }

            var dbPosts = await _dbContext.BlogPosts
                .AsNoTracking()
                .OrderByDescending(a => a.PublishDate)
                .Skip(page * limit)
                .Take(limit)
                .ToListAsync();

            var authorUserIds = dbPosts.Select(a => a.AuthorId).Distinct();
            var authorNames = await _userManagementClient.GetDisplayNames(authorUserIds);

            blogPosts = dbPosts.Select(p =>
            {
                var author = authorNames.FirstOrDefault(a => a.UserId == p.AuthorId);

                return new BlogPost
                {
                    Id = p.Id,
                    Title = p.Title,
                    Author = author?.Name ?? "Unknown",
                    PublishDate = p.PublishDate,
                    Content = p.Content
                };
            });

            if (blogPosts != null && blogPosts.Any())
            {
                await _cache.SetAsync(cacheKey, blogPosts, _blogPostListCacheExpiration);
            }

            return blogPosts;
        }

        public async Task<BlogPost> UpdateBlogPost(string postId, BlogPost blogPost)
        {
            var dbPost = await _dbContext.BlogPosts.SingleOrDefaultAsync(a => a.Id == postId);

            dbPost.Title = blogPost.Title;
            dbPost.Content = blogPost.Content;

            await _dbContext.SaveChangesAsync();

            return blogPost;
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
