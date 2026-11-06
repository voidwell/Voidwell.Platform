using AutoMapper;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Clients.Keycloak;
using Voidwell.Platform.Clients.Keycloak.Models;
using Voidwell.Platform.Data.Repositories;

namespace Voidwell.Platform.Api.Services;

public class BlogPostService : IBlogPostService
{
    /// <summary>Author name used when the display name lookup fails, so posts still load.</summary>
    public const string FallbackAuthorName = "";

    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;
    private readonly IKeycloakClient _keycloakClient;
    private readonly ILogger<BlogPostService> _logger;

    public BlogPostService(IBlogPostRepository blogPostRepository, IMapper mapper, IKeycloakClient keycloakClient, ILogger<BlogPostService> logger)
    {
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
        _keycloakClient = keycloakClient;
        _logger = logger;
    }

    public async Task<IEnumerable<BlogPost>> GetBlogPostsByPageAsync(int page)
    {
        var storeBlogPosts = await _blogPostRepository.GetBlogPostsAsync(page, 10);

        var authorMap = storeBlogPosts.ToDictionary(a => a.Id, a => a.AuthorId);
        var blogPosts = _mapper.Map<IEnumerable<BlogPost>>(storeBlogPosts).ToList();

        IEnumerable<DisplayName> authorNames;
        try
        {
            authorNames = await _keycloakClient.GetDisplayNamesAsync(authorMap.Values.Distinct());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve blog post author names");
            blogPosts.ForEach(p => p.AuthorName = FallbackAuthorName);
            return blogPosts;
        }

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
        try
        {
            blogPost.AuthorName = (await _keycloakClient.GetDisplayNameAsync(storeBlogPost.AuthorId))?.Name;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve author name for blog post {BlogPostId}", blogPostId);
            blogPost.AuthorName = FallbackAuthorName;
        }

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

    public async Task<BlogPost?> UpdateBlogPostAsync(Guid blogPostId, BlogPostRequest blogPostRequest)
    {
        var mappedBlogPost = _mapper.Map<Data.Models.BlogPost>(blogPostRequest);

        var storeBlogPost = await _blogPostRepository.GetBlogPostAsync(blogPostId);
        if (storeBlogPost == null)
        {
            return null;
        }

        storeBlogPost.Title = mappedBlogPost.Title;
        storeBlogPost.MarkdownContent = mappedBlogPost.MarkdownContent;
        storeBlogPost.HtmlContent = mappedBlogPost.HtmlContent;
        storeBlogPost.BlogPostTagMaps = mappedBlogPost.BlogPostTagMaps;

        await _blogPostRepository.UpdateBlogPostAsync(storeBlogPost);

        return await GetBlogPostByIdAsync(blogPostId);
    }

    public Task DeleteBlogPostAsync(Guid blogPostId)
    {
        return _blogPostRepository.DeleteBlogPostAsync(blogPostId);
    }

    public async Task<EditableBlogPost?> GetEditableBlogPostByIdAsync(Guid blogPostId)
    {
        var storeBlogPost = await _blogPostRepository.GetBlogPostAsync(blogPostId);
        if (storeBlogPost == null)
        {
            return null;
        }

        return _mapper.Map<EditableBlogPost>(storeBlogPost);
    }
}
