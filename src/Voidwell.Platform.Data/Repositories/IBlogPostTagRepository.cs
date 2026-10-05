using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.Repositories;

public interface IBlogPostTagRepository
{
    Task<IEnumerable<BlogPostTag>> GetTagsStartingWithValue(string value);
    Task<Guid> CreateTag(string tagName);
}
