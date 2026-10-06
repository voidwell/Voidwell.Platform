using System.Reflection;
using AutoMapper;
using FluentAssertions;
using Markdig;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Voidwell.Platform.Api.Services;
using Voidwell.Platform.Api.Services.Mappers;
using Voidwell.Platform.Clients.Keycloak;
using Voidwell.Platform.Clients.Keycloak.Models;
using Voidwell.Platform.Data.Repositories;
using Xunit;

namespace Voidwell.Platform.Api.Test.Services;

public class BlogPostServiceTest
{
    private IBlogPostRepository _mockBlogPostRepository;
    private IKeycloakClient _mockKeycloakClient;

    private IBlogPostService _subject;

    public BlogPostServiceTest()
    {
        _mockBlogPostRepository = Mock.Of<IBlogPostRepository>();
        _mockKeycloakClient = Mock.Of<IKeycloakClient>();
        var mapper = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetAssembly(typeof(BlogPostMapper))), NullLoggerFactory.Instance)
            .CreateMapper();

        _subject = new BlogPostService(_mockBlogPostRepository, mapper, _mockKeycloakClient);
    }

    [Fact]
    public async Task GetBlogPostsByPageAsync_ReturnsBlogPosts()
    {
        var expectedPage = 1;

        var testBlogPost1 = GenerateTestBlogPost();
        var testBlogPost2 = GenerateTestBlogPost(authorId: testBlogPost1.Item2.AuthorId);
        var expectedBlogPosts = new[] {
            testBlogPost1.Item1,
            testBlogPost2.Item1,
        };
        var storeBlogPosts = new[]
        {
            testBlogPost1.Item2,
            testBlogPost2.Item2,
        };

        _mockBlogPostRepository.AsMock()
            .Setup(a => a.GetBlogPostsAsync(expectedPage, It.IsAny<int>()))
            .ReturnsAsync(storeBlogPosts)
            .Verifiable();
        _mockKeycloakClient.AsMock()
            .Setup(a => a.GetDisplayNamesAsync(new[] { testBlogPost1.Item2.AuthorId }))
            .ReturnsAsync(new[] { new DisplayName { UserId = testBlogPost1.Item2.AuthorId, Name = testBlogPost1.Item1.AuthorName } })
            .Verifiable();

        var result = await _subject.GetBlogPostsByPageAsync(expectedPage);

        result.Should()
            .BeEquivalentTo(expectedBlogPosts);

        _mockBlogPostRepository.AsMock().Verify();
        _mockKeycloakClient.AsMock().Verify();
    }

    [Fact]
    public async Task GetBlogPostByIdAsync_ReturnsBlogPost()
    {
        var expectedBlogPostId = Guid.NewGuid();

        var testBlogPost = GenerateTestBlogPost(blogPostId: expectedBlogPostId);
        var expectedBlogPost = testBlogPost.Item1;
        var storeBlogPost = testBlogPost.Item2;

        _mockBlogPostRepository.AsMock()
            .Setup(a => a.GetBlogPostAsync(expectedBlogPostId))
            .ReturnsAsync(storeBlogPost)
            .Verifiable();
        _mockKeycloakClient.AsMock()
            .Setup(a => a.GetDisplayNameAsync(testBlogPost.Item2.AuthorId))
            .ReturnsAsync(new DisplayName { UserId = testBlogPost.Item2.AuthorId, Name = expectedBlogPost.AuthorName })
            .Verifiable();

        var result = await _subject.GetBlogPostByIdAsync(expectedBlogPostId);

        result.Should()
            .BeEquivalentTo(expectedBlogPost);

        _mockBlogPostRepository.AsMock().Verify();
        _mockKeycloakClient.AsMock().Verify();
    }

    [Fact]
    public async Task CreateBlogPostAsync_ReturnsBlogPost()
    {
        var expectedBlogPostId = Guid.NewGuid();
        var createAuthorId = Guid.NewGuid();
        var testBlogPost = GenerateTestBlogPost(expectedBlogPostId, createAuthorId);
        var expectedBlogPost = testBlogPost.Item1;
        var storeBlogPost = testBlogPost.Item2;

        var blogPostRequest = new Models.BlogPostRequest
        {
            MarkdownContent = storeBlogPost.MarkdownContent,
            Title = storeBlogPost.Title,
            Tags = expectedBlogPost.Tags
        };

        _mockBlogPostRepository.AsMock()
            .Setup(a => a.CreateBlogPostAsync(It.IsAny<Data.Models.BlogPost>()))
            .ReturnsAsync(expectedBlogPostId);
        _mockBlogPostRepository.AsMock()
            .Setup(a => a.GetBlogPostAsync(expectedBlogPostId))
            .ReturnsAsync(storeBlogPost);
        _mockKeycloakClient.AsMock()
            .Setup(a => a.GetDisplayNameAsync(createAuthorId))
            .ReturnsAsync(new DisplayName { UserId = createAuthorId, Name = expectedBlogPost.AuthorName })
            .Verifiable();

        var result = await _subject.CreateBlogPostAsync(createAuthorId, blogPostRequest);

        result.Should()
            .BeEquivalentTo(expectedBlogPost);

        _mockBlogPostRepository.AsMock().Verify(a => a.CreateBlogPostAsync(It.Is<Data.Models.BlogPost>(a => a.AuthorId == createAuthorId && a.PublishDate != default)), Times.Once);
        _mockBlogPostRepository.AsMock().Verify(a => a.GetBlogPostAsync(expectedBlogPostId), Times.Once);
        _mockKeycloakClient.AsMock().Verify();
    }

    [Fact]
    public async Task UpdateBlogPostAsync_ReturnsBlogPost()
    {
        var expectedBlogPostId = Guid.NewGuid();
        var storeTestBlogPost = GenerateTestBlogPost(expectedBlogPostId);
        var updatedTestBlogPost = GenerateTestBlogPost(expectedBlogPostId, storeTestBlogPost.Item2.AuthorId);
        var storeBlogPost = storeTestBlogPost.Item2;
        var updatedStoreBlogPost = updatedTestBlogPost.Item2;
        var expectedUpdatedBlogPost = updatedTestBlogPost.Item1;

        var blogPostRequest = new Models.BlogPostRequest
        {
            MarkdownContent = storeBlogPost.MarkdownContent,
            Title = "Updated Title",
            Tags = storeTestBlogPost.Item1.Tags
        };

        updatedStoreBlogPost.Title = blogPostRequest.Title;
        expectedUpdatedBlogPost.Title = updatedStoreBlogPost.Title;

        _mockBlogPostRepository.AsMock()
            .Setup(a => a.UpdateBlogPostAsync(It.IsAny<Data.Models.BlogPost>()));
        _mockBlogPostRepository.AsMock()
            .SetupSequence(a => a.GetBlogPostAsync(It.IsAny<Guid>()))
            .ReturnsAsync(storeBlogPost)
            .ReturnsAsync(updatedStoreBlogPost);
        _mockKeycloakClient.AsMock()
            .Setup(a => a.GetDisplayNameAsync(storeBlogPost.AuthorId))
            .ReturnsAsync(new DisplayName { UserId = storeBlogPost.AuthorId, Name = expectedUpdatedBlogPost.AuthorName })
            .Verifiable();

        var result = await _subject.UpdateBlogPostAsync(expectedBlogPostId, blogPostRequest);

        result.Should()
            .BeEquivalentTo(expectedUpdatedBlogPost);

        _mockBlogPostRepository.AsMock().Verify(a => a.UpdateBlogPostAsync(It.Is<Data.Models.BlogPost>(x => x.Id == expectedBlogPostId && x.Title == blogPostRequest.Title)), Times.Once);
        _mockBlogPostRepository.AsMock().Verify(a => a.GetBlogPostAsync(expectedBlogPostId), Times.Exactly(2));
        _mockKeycloakClient.AsMock().Verify();
    }

    [Fact]
    public async Task DeleteBlogPostAsync_DeletesBlogPost()
    {
        var expectedBlogPostId = Guid.NewGuid();

        _mockBlogPostRepository.AsMock()
            .Setup(a => a.DeleteBlogPostAsync(expectedBlogPostId))
            .Verifiable();

        await _subject.DeleteBlogPostAsync(expectedBlogPostId);

        _mockBlogPostRepository.AsMock().Verify();
    }

    [Fact]
    public async Task GetEditableBlogPostByIdAsync_ReturnsEditableBlogPost()
    {
        var expectedBlogPostId = Guid.NewGuid();
        var testBlogPost = GenerateTestBlogPost(expectedBlogPostId);
        var storeBlogPost = testBlogPost.Item2;
        var expectedEditableBlogPost = new Models.EditableBlogPost
        {
            Id = storeBlogPost.Id,
            MarkdownContent = storeBlogPost.MarkdownContent,
            Title = storeBlogPost.Title,
            Tags = testBlogPost.Item1.Tags
        };

        _mockBlogPostRepository.AsMock()
            .Setup(a => a.GetBlogPostAsync(expectedBlogPostId))
            .ReturnsAsync(storeBlogPost);

        var result = await _subject.GetEditableBlogPostByIdAsync(expectedBlogPostId);

        result.Should()
            .BeEquivalentTo(expectedEditableBlogPost);

        _mockBlogPostRepository.AsMock().Verify(a => a.GetBlogPostAsync(expectedBlogPostId), Times.Once);
    }

    private static Tuple<Models.BlogPost, Data.Models.BlogPost> GenerateTestBlogPost(Guid? blogPostId = null, Guid? authorId = null)
    {
        blogPostId ??= Guid.NewGuid();
        authorId ??= Guid.NewGuid();

        var publishDate = DateTimeOffset.UtcNow;
        var title = $"{blogPostId}-title";
        var markdownContent = $"{blogPostId}-content";
        var htmlContent = Markdown.ToHtml(markdownContent);

        var storeBlogPost = new Data.Models.BlogPost
        {
            Id = blogPostId.Value,
            AuthorId = authorId.Value,
            PublishDate = publishDate,
            Title = title,
            MarkdownContent = markdownContent,
            HtmlContent = htmlContent,
            BlogPostTagMaps = Enumerable.Empty<Data.Models.BlogPostTagMap>()
        };

        var expectedBlogPost = new Models.BlogPost
        {
            Id = blogPostId.Value,
            AuthorName = $"{authorId}-author",
            PublishDate = publishDate,
            Title = title,
            HtmlContent = htmlContent,
            Tags = Enumerable.Empty<Models.BlogPostTag>()
        };

        return Tuple.Create(expectedBlogPost, storeBlogPost);
    }
}
