using Microsoft.EntityFrameworkCore;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.Repositories;

public class BlogPostRepository : IBlogPostRepository
{
    private readonly IDbContextFactory<VoidwellDbContext> _dbContextFactory;

    public BlogPostRepository(IDbContextFactory<VoidwellDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<BlogPost>> GetBlogPostsAsync(int page, int limit)
    {
        using var context = _dbContextFactory.CreateDbContext();

        return await context.BlogPosts
            .Include(a => a.BlogPostTagMaps!)
                .ThenInclude(a => a.BlogPostTag)
            .OrderByDescending(a => a.PublishDate)
            .ThenBy(a => a.Id)
            .Skip(page * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetBlogPostAsync(Guid blogPostId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        return await context.BlogPosts
            .Include(a => a.BlogPostTagMaps!)
                .ThenInclude(a => a.BlogPostTag)
            .FirstOrDefaultAsync(a => a.Id == blogPostId);
    }

    public async Task<Guid> CreateBlogPostAsync(BlogPost blogPost)
    {
        using var context = _dbContextFactory.CreateDbContext();

        blogPost.Id = Guid.NewGuid();

        context.BlogPosts.Add(blogPost);

        await context.SaveChangesAsync();

        return blogPost.Id;
    }

    public async Task UpdateBlogPostAsync(BlogPost blogPost)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var storeBlogPost = await context.BlogPosts.FirstOrDefaultAsync(a => a.Id == blogPost.Id)
            ?? throw new InvalidOperationException($"Blog post {blogPost.Id} does not exist.");

        storeBlogPost.Id = blogPost.Id;
        storeBlogPost.PublishDate = blogPost.PublishDate;
        storeBlogPost.AuthorId = blogPost.AuthorId;
        storeBlogPost.Title = blogPost.Title;
        storeBlogPost.MarkdownContent = blogPost.MarkdownContent;
        storeBlogPost.HtmlContent = blogPost.HtmlContent;
        storeBlogPost.BlogPostTagMaps = blogPost.BlogPostTagMaps;

        await context.SaveChangesAsync();
    }

    public async Task DeleteBlogPostAsync(Guid blogPostId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var storeBlogPost = await context.BlogPosts.FirstOrDefaultAsync(a => a.Id == blogPostId);
        if (storeBlogPost == null)
        {
            return;
        }

        context.Remove(storeBlogPost);

        await context.SaveChangesAsync();
    }
}
