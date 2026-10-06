using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voidwell.Platform.Api.Authentication;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Api.Services;

namespace Voidwell.Platform.Api.Controllers;

[ApiController]
[Route("post")]
public class PostController : ControllerBase
{
    private readonly IBlogPostService _blogPostService;
    private readonly IUserHelper _userHelper;

    public PostController(IBlogPostService blogPostService, IUserHelper userHelper)
    {
        _blogPostService = blogPostService;
        _userHelper = userHelper;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllPosts([FromQuery] int page = 0)
    {
        var blogPosts = await _blogPostService.GetBlogPostsByPageAsync(page);
        return Ok(blogPosts);
    }

    [HttpGet("{blogPostId:guid}")]
    public async Task<ActionResult> GetPost(Guid blogPostId)
    {
        var blogPost = await _blogPostService.GetBlogPostByIdAsync(blogPostId);
        if (blogPost == null)
        {
            return NotFound();
        }

        return Ok(blogPost);
    }

    [HttpPost]
    [Authorize(Roles = AuthConstants.Roles.Administrator)]
    public async Task<ActionResult> CreatePost([FromBody] BlogPostRequest blogPostRequest)
    {
        var userId = _userHelper.GetUserId();
        if (userId == null)
        {
            return BadRequest("Could not resolve user id");
        }

        var blogPost = await _blogPostService.CreateBlogPostAsync(userId.Value, blogPostRequest);
        return Ok(blogPost);
    }

    [HttpDelete("{blogPostId:guid}")]
    [Authorize(Roles = AuthConstants.Roles.Administrator)]
    public async Task<ActionResult> DeletePost(Guid blogPostId)
    {
        await _blogPostService.DeleteBlogPostAsync(blogPostId);
        return NoContent();
    }

    [HttpGet("edit/{blogPostId:guid}")]
    [Authorize(Roles = AuthConstants.Roles.Administrator)]
    public async Task<ActionResult> GetEditablePost(Guid blogPostId)
    {
        var blogPost = await _blogPostService.GetEditableBlogPostByIdAsync(blogPostId);
        if (blogPost == null)
        {
            return NotFound();
        }

        return Ok(blogPost);
    }

    [HttpPut("edit/{blogPostId:guid}")]
    [Authorize(Roles = AuthConstants.Roles.Administrator)]
    public async Task<ActionResult> UpdatePost(Guid blogPostId, [FromBody] BlogPostRequest blogPostRequest)
    {
        var blogPost = await _blogPostService.UpdateBlogPostAsync(blogPostId, blogPostRequest);
        if (blogPost == null)
        {
            return NotFound();
        }

        return Ok(blogPost);
    }
}
