using Microsoft.EntityFrameworkCore;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.Repositories;

public class BlogPostTagRepository : IBlogPostTagRepository
{
    private readonly IDbContextFactory<VoidwellDbContext> _dbContextFactory;

    private readonly Func<string, string> _normalizeName = name => name.ToUpper();

    public BlogPostTagRepository(IDbContextFactory<VoidwellDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<BlogPostTag>> GetTagsStartingWithValue(string value)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var normalizedName = _normalizeName(value);
        return await context.BlogPostTags
            .Where(a => a.NormalizedName == normalizedName)
            .ToListAsync();
    }

    public async Task<Guid> CreateTag(string tagName)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var blogPostTag = new BlogPostTag
        {
            Id = Guid.NewGuid(),
            Name = tagName,
            NormalizedName = _normalizeName(tagName)
        };

        context.BlogPostTags.Add(blogPostTag);

        await context.SaveChangesAsync();

        return blogPostTag.Id;
    }
}
