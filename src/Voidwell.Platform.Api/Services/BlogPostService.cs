using AutoMapper;
using Voidwell.Platform.Api.Clients;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Data.Repositories;

namespace Voidwell.Platform.Api.Services;

public class BlogPostService : IBlogPostService
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;
    private readonly IKeycloakClient _keycloakClient;

    public BlogPostService(IBlogPostRepository blogPostRepository, IMapper mapper, IKeycloakClient keycloakClient)
    {
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
        _keycloakClient = keycloakClient;
    }

    public async Task<IEnumerable<BlogPost>> GetBlogPostsByPageAsync(int page)
    {
        var storeBlogPosts = await _blogPostRepository.GetBlogPostsAsync(page, 10);

        var authorMap = storeBlogPosts.ToDictionary(a => a.Id, a => a.AuthorId);
        var authorNames = await _keycloakClient.GetDisplayNamesAsync(authorMap.Values.Distinct());

        var blogPosts = _mapper.Map<IEnumerable<BlogPost>>(storeBlogPosts).ToList();
        blogPosts.ForEach(p => p.AuthorName = authorNames.FirstOrDefault(n => n.UserId == authorMap[p.Id])?.Name);

        return blogPosts;
    }

    public async Task<BlogPost?> GetBlogPostByIdAsync(Guid blogPostId)
    {
        var storeBlogPost = await _blogPostRepository.GetBlogPostAsync(blogPostId);
        if (storeBlogPost == null)
        {
            return null;
        }

        var blogPost = _mapper.Map<BlogPost>(storeBlogPost);
        blogPost.AuthorName = (await _keycloakClient.GetDisplayNameAsync(storeBlogPost.AuthorId))?.Name;

        return blogPost;
    }

    public async Task<BlogPost> CreateBlogPostAsync(Guid authorId, BlogPostRequest blogPostRequest)
    {
        var newBlogPost = _mapper.Map<Data.Models.BlogPost>(blogPostRequest);

        newBlogPost.AuthorId = authorId;
        newBlogPost.PublishDate = DateTimeOffset.UtcNow;

        var blogPostId = await _blogPostRepository.CreateBlogPostAsync(newBlogPost);

        return await GetBlogPostByIdAsync(blogPostId)
            ?? throw new InvalidOperationException($"Blog post {blogPostId} was not found after it was created.");
    }

    public async Task<BlogPost?> UpdateBlogPostAsync(BlogPostRequest blogPostRequest)
    {
        var mappedBlogPost = _mapper.Map<Data.Models.BlogPost>(blogPostRequest);

        var storeBlogPost = await _blogPostRepository.GetBlogPostAsync(blogPostRequest.Id);
        if (storeBlogPost == null)
        {
            return null;
        }

        storeBlogPost.Title = mappedBlogPost.Title;
        storeBlogPost.MarkdownContent = mappedBlogPost.MarkdownContent;
        storeBlogPost.HtmlContent = mappedBlogPost.HtmlContent;
        storeBlogPost.BlogPostTagMaps = mappedBlogPost.BlogPostTagMaps;

        await _blogPostRepository.UpdateBlogPostAsync(storeBlogPost);

        return await GetBlogPostByIdAsync(blogPostRequest.Id);
    }

    public Task DeleteBlogPostAsync(Guid blogPostId)
    {
        return _blogPostRepository.DeleteBlogPostAsync(blogPostId);
    }

    public async Task<BlogPostRequest?> GetEditableBlogPostByIdAsync(Guid blogPostId)
    {
        var storeBlogPost = await _blogPostRepository.GetBlogPostAsync(blogPostId);
        if (storeBlogPost == null)
        {
            return null;
        }

        return _mapper.Map<BlogPostRequest>(storeBlogPost);
    }
}
