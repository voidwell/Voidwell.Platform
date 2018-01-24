using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Voidwell.Internal.Models;
using Voidwell.Internal.Services;

namespace Voidwell.Internal.Controllers
{
    [Route("blog")]
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly IUserHelper _userHelper;

        public BlogController(IBlogService blogService, IUserHelper userHelper)
        {
            _blogService = blogService;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllBlogPosts()
        {
            var result = await _blogService.GetBlogPosts(10, 0);
            return Ok(result);
        }

        [HttpGet("{postId}")]
        public async Task<ActionResult> GetBlogPost(string postId)
        {
            var result = await _blogService.GetBlogPost(postId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> PostBlogPost([FromBody]BlogPost blogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _userHelper.GetUserId();
            if (userId == null)
            {
                return BadRequest("Could not resolve user id");
            }

            var result = await _blogService.CreateBlogPost(userId.Value, blogPost);
            return Created("blog", result);
        }

        [HttpPut("{postId}")]
        public async Task<ActionResult> PutBlogPost(string postId, [FromBody]BlogPost blogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _blogService.UpdateBlogPost(postId, blogPost);
            return Ok(result);
        }

        [HttpDelete("{postId}")]
        public async Task<ActionResult> DeleteBlogPost(string postId)
        {
            await _blogService.DeleteBlogPost(postId);
            return NoContent();
        }
    }
}