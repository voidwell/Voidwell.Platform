using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Voidwell.Platform.Api.Controllers;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Api.Services;
using Xunit;

namespace Voidwell.Platform.Api.Test.Controllers;

public class PostControllerTest
{
    private readonly Mock<IBlogPostService> _blogPostService = new();
    private readonly Mock<IUserHelper> _userHelper = new();
    private readonly PostController _subject;

    public PostControllerTest()
    {
        _subject = new PostController(_blogPostService.Object, _userHelper.Object);
    }

    [Fact]
    public async Task GetAllPosts_ReturnsPostsForPage()
    {
        var posts = new[] { new BlogPost { Id = Guid.NewGuid() } };
        _blogPostService.Setup(a => a.GetBlogPostsByPageAsync(2)).ReturnsAsync(posts);

        var result = await _subject.GetAllPosts(2);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(posts);
    }

    [Fact]
    public async Task GetPost_ReturnsPost()
    {
        var post = new BlogPost { Id = Guid.NewGuid() };
        _blogPostService.Setup(a => a.GetBlogPostByIdAsync(post.Id)).ReturnsAsync(post);

        var result = await _subject.GetPost(post.Id);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(post);
    }

    [Fact]
    public async Task GetPost_ReturnsNotFound_WhenPostDoesNotExist()
    {
        var result = await _subject.GetPost(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreatePost_ReturnsBadRequest_WhenUserCannotBeResolved()
    {
        _userHelper.Setup(a => a.GetUserId()).Returns((Guid?)null);

        var result = await _subject.CreatePost(new BlogPostRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
        _blogPostService.Verify(a => a.CreateBlogPostAsync(It.IsAny<Guid>(), It.IsAny<BlogPostRequest>()), Times.Never);
    }

    [Fact]
    public async Task CreatePost_CreatesPostForCurrentUser()
    {
        var userId = Guid.NewGuid();
        var request = new BlogPostRequest { Title = "Title", MarkdownContent = "Content" };
        var post = new BlogPost { Id = Guid.NewGuid() };
        _userHelper.Setup(a => a.GetUserId()).Returns(userId);
        _blogPostService.Setup(a => a.CreateBlogPostAsync(userId, request)).ReturnsAsync(post);

        var result = await _subject.CreatePost(request);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(post);
    }

    [Fact]
    public async Task DeletePost_DeletesAndReturnsNoContent()
    {
        var postId = Guid.NewGuid();

        var result = await _subject.DeletePost(postId);

        result.Should().BeOfType<NoContentResult>();
        _blogPostService.Verify(a => a.DeleteBlogPostAsync(postId), Times.Once);
    }

    [Fact]
    public async Task GetEditablePost_ReturnsEditablePost()
    {
        var request = new BlogPostRequest { Id = Guid.NewGuid() };
        _blogPostService.Setup(a => a.GetEditableBlogPostByIdAsync(request.Id)).ReturnsAsync(request);

        var result = await _subject.GetEditablePost(request.Id);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(request);
    }

    [Fact]
    public async Task GetEditablePost_ReturnsNotFound_WhenPostDoesNotExist()
    {
        var result = await _subject.GetEditablePost(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdatePost_AppliesRouteIdToRequest()
    {
        var postId = Guid.NewGuid();
        var request = new BlogPostRequest { Id = Guid.NewGuid(), Title = "Title", MarkdownContent = "Content" };
        var post = new BlogPost { Id = postId };
        _blogPostService.Setup(a => a.UpdateBlogPostAsync(It.Is<BlogPostRequest>(r => r.Id == postId))).ReturnsAsync(post);

        var result = await _subject.UpdatePost(postId, request);

        request.Id.Should().Be(postId);
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(post);
    }

    [Fact]
    public async Task UpdatePost_ReturnsNotFound_WhenPostDoesNotExist()
    {
        var result = await _subject.UpdatePost(Guid.NewGuid(), new BlogPostRequest());

        result.Should().BeOfType<NotFoundResult>();
    }
}
