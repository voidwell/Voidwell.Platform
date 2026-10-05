using FluentAssertions;
using Voidwell.Platform.Data.Repositories;
using Xunit;

namespace Voidwell.Platform.Data.Test;

public class BlogPostTagRepositoryTest
{
    private readonly BlogPostTagRepository _subject;

    public BlogPostTagRepositoryTest()
    {
        _subject = new BlogPostTagRepository(new TestDbContextFactory());
    }

    [Fact]
    public async Task CreateTag_StoresNormalizedName()
    {
        var id = await _subject.CreateTag("Some Tag");

        var tags = await _subject.GetTagsStartingWithValue("some tag");

        var tag = tags.Should().ContainSingle().Subject;
        tag.Id.Should().Be(id);
        tag.Name.Should().Be("Some Tag");
        tag.NormalizedName.Should().Be("SOME TAG");
    }

    [Fact]
    public async Task GetTagsStartingWithValue_ReturnsNothing_WhenNoTagMatches()
    {
        await _subject.CreateTag("one");

        var tags = await _subject.GetTagsStartingWithValue("two");

        tags.Should().BeEmpty();
    }
}
