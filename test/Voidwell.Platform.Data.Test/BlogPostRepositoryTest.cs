using FluentAssertions;
using Voidwell.Platform.Data.Models;
using Voidwell.Platform.Data.Repositories;
using Xunit;

namespace Voidwell.Platform.Data.Test;

public class BlogPostRepositoryTest
{
    private readonly BlogPostRepository _subject;

    public BlogPostRepositoryTest()
    {
        _subject = new BlogPostRepository(new TestDbContextFactory());
    }

    [Fact]
    public async Task CreateBlogPostAsync_AssignsIdAndPersistsPost()
    {
        var post = CreatePost();

        var id = await _subject.CreateBlogPostAsync(post);

        id.Should().NotBeEmpty();
        var loaded = await _subject.GetBlogPostAsync(id);
        loaded.Should().NotBeNull();
        loaded.Title.Should().Be(post.Title);
        loaded.AuthorId.Should().Be(post.AuthorId);
    }

    [Fact]
    public async Task GetBlogPostAsync_IncludesTags()
    {
        var tagId = Guid.NewGuid();
        var post = CreatePost();
        post.BlogPostTagMaps =
        [
            new BlogPostTagMap
            {
                BlogPostTagId = tagId,
                BlogPostTag = new BlogPostTag { Id = tagId, Name = "Tag", NormalizedName = "TAG" }
            }
        ];

        var id = await _subject.CreateBlogPostAsync(post);

        var loaded = await _subject.GetBlogPostAsync(id);
        loaded.BlogPostTagMaps.Should().ContainSingle()
            .Which.BlogPostTag.Name.Should().Be("Tag");
    }

    [Fact]
    public async Task GetBlogPostAsync_ReturnsNull_WhenPostDoesNotExist()
    {
        var result = await _subject.GetBlogPostAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBlogPostAsync_UpdatesStoredPost()
    {
        var id = await _subject.CreateBlogPostAsync(CreatePost());
        var existing = await _subject.GetBlogPostAsync(id);
        existing.Title = "Updated";
        existing.MarkdownContent = "updated content";

        await _subject.UpdateBlogPostAsync(existing);

        var loaded = await _subject.GetBlogPostAsync(id);
        loaded.Title.Should().Be("Updated");
        loaded.MarkdownContent.Should().Be("updated content");
    }

    [Fact]
    public async Task UpdateBlogPostAsync_Throws_WhenPostDoesNotExist()
    {
        var act = () => _subject.UpdateBlogPostAsync(CreatePost());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteBlogPostAsync_RemovesPost()
    {
        var id = await _subject.CreateBlogPostAsync(CreatePost());

        await _subject.DeleteBlogPostAsync(id);

        (await _subject.GetBlogPostAsync(id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteBlogPostAsync_DoesNothing_WhenPostDoesNotExist()
    {
        var act = () => _subject.DeleteBlogPostAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    private static BlogPost CreatePost()
    {
        return new BlogPost
        {
            Title = "Title",
            AuthorId = Guid.NewGuid(),
            PublishDate = DateTimeOffset.UtcNow,
            MarkdownContent = "content",
            HtmlContent = "<p>content</p>"
        };
    }
}
