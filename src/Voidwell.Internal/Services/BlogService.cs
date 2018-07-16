using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voidwell.Cache;
using Voidwell.Internal.Clients;
using Voidwell.Internal.Data.Models;
using Voidwell.Internal.Data.Repositories;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Services
{
    public class BlogService : IBlogService
    {
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly IUserManagementClient _userManagementClient;
        private readonly ICache _cache;

        private readonly string _cacheKey = "blog";
        private readonly TimeSpan _blogPostListCacheExpiration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _blogPostCacheExpiration = TimeSpan.FromMinutes(10);

        public BlogService(IBlogPostRepository blogPostRepository, IUserManagementClient userManagementClient, ICache cache)
        {
            _blogPostRepository = blogPostRepository;
            _userManagementClient = userManagementClient;
            _cache = cache;
        }

        public Task<BlogPost> CreateBlogPostAsync(Guid authorId, BlogPost blogPost)
        {
            blogPost.PublishDate = DateTime.UtcNow;
            blogPost.AuthorId = authorId;
            return _blogPostRepository.CreateBlogPostAsync(blogPost);
        }

        public Task DeleteBlogPostAsync(string postId)
        {
            return _blogPostRepository.RemoveBlogPostAsync(postId);
        }

        public async Task<BlogPostModel> GetBlogPostAsync(string postId)
        {
            var cacheKey = $"{_cacheKey}_post_{postId}";

            var model = await _cache.GetAsync<BlogPostModel>(cacheKey);
            if (model != null)
            {
                return model;
            }

            var blogPost = await _blogPostRepository.GetBlogPostAsync(postId);
            if (blogPost != null)
            {
                var author = await _userManagementClient.GetDisplayName(blogPost.AuthorId);

                model = new BlogPostModel
                {
                    Id = blogPost.Id,
                    Title = blogPost.Title,
                    PublishDate = blogPost.PublishDate,
                    Author = author?.Name ?? "Unknown",
                    Content = blogPost.Content
                };

                await _cache.SetAsync(cacheKey, model, _blogPostCacheExpiration);
            }

            return model;
        }

        public async Task<IEnumerable<BlogPostModel>> GetBlogPostsAsync(int page, int limit)
        {
            var cacheKey = $"{_cacheKey}_postlist_{limit}_{page}";

            var models = await _cache.GetAsync<IEnumerable<BlogPostModel>>(cacheKey);
            if (models != null)
            {
                return models;
            }

            var blogPosts = await _blogPostRepository.GetAllBlogPostsAsync(limit * page, limit);
            if (blogPosts == null || !blogPosts.Any())
            {
                return null;
            }

            var authorUserIds = blogPosts.Select(a => a.AuthorId).Distinct();
            var authorNames = await _userManagementClient.GetDisplayNames(authorUserIds);

            models = blogPosts.Select(p =>
            {
                var author = authorNames.FirstOrDefault(a => a.UserId == p.AuthorId);

                return new BlogPostModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Author = author?.Name ?? "Unknown",
                    PublishDate = p.PublishDate,
                    Content = p.Content
                };
            });

            await _cache.SetAsync(cacheKey, models, _blogPostListCacheExpiration);

            return models;
        }

        public Task<BlogPost> UpdateBlogPostAsync(string postId, BlogPost blogPost)
        {
            return _blogPostRepository.UpdateBlogPostAsync(blogPost);
        }
    }
}
