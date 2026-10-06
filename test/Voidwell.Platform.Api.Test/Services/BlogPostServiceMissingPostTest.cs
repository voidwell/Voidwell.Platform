using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Api.Services;
using Voidwell.Platform.Api.Services.Mappers;
using Voidwell.Platform.Clients.Keycloak;
using Voidwell.Platform.Data.Repositories;
using Xunit;

namespace Voidwell.Platform.Api.Test.Services;

public class BlogPostServiceMissingPostTest
{
    private readonly Mock<IBlogPostRepository> _repository = new();
    private readonly Mock<IKeycloakClient> _keycloakClient = new();
    private readonly BlogPostService _subject;

    public BlogPostServiceMissingPostTest()
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddMaps(typeof(BlogPostMapper).Assembly), NullLoggerFactory.Instance)
            .CreateMapper();

        _subject = new BlogPostService(_repository.Object, mapper, _keycloakClient.Object);
    }

    [Fact]
    public async Task GetBlogPostByIdAsync_ReturnsNull_WhenPostDoesNotExist()
    {
        var result = await _subject.GetBlogPostByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
        _keycloakClient.Verify(a => a.GetDisplayNameAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetEditableBlogPostByIdAsync_ReturnsNull_WhenPostDoesNotExist()
    {
        var result = await _subject.GetEditableBlogPostByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBlogPostAsync_ReturnsNull_WhenPostDoesNotExist()
    {
        var request = new BlogPostRequest { Title = "Title", MarkdownContent = "Content" };

        var result = await _subject.UpdateBlogPostAsync(Guid.NewGuid(), request);

        result.Should().BeNull();
        _repository.Verify(a => a.UpdateBlogPostAsync(It.IsAny<Data.Models.BlogPost>()), Times.Never);
    }

    [Fact]
    public async Task GetBlogPostsByPageAsync_LeavesAuthorNameEmpty_WhenUserIsNotFound()
    {
        var blogPost = new Data.Models.BlogPost
        {
            Id = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Title = "Title",
            HtmlContent = "<p>Content</p>",
            MarkdownContent = "Content",
            PublishDate = DateTimeOffset.UtcNow
        };
        _repository.Setup(a => a.GetBlogPostsAsync(0, 10)).ReturnsAsync([blogPost]);
        _keycloakClient.Setup(a => a.GetDisplayNamesAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync([]);

        var result = (await _subject.GetBlogPostsByPageAsync(0)).ToList();

        result.Should().ContainSingle()
            .Which.AuthorName.Should().BeNull();
    }

    [Fact]
    public async Task CreateBlogPostAsync_Throws_WhenCreatedPostCannotBeReloaded()
    {
        var request = new BlogPostRequest { Title = "Title", MarkdownContent = "Content" };
        _repository.Setup(a => a.CreateBlogPostAsync(It.IsAny<Data.Models.BlogPost>())).ReturnsAsync(Guid.NewGuid());

        var act = () => _subject.CreateBlogPostAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
