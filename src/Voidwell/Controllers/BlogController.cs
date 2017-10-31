using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Voidwell.Models;
using Voidwell.Services;

namespace Voidwell.Controllers
{
    [Route("blog")]
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllBlogPosts()
        {
            var result = await _blogService.GetBlogPosts();
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

            blogPost.Author = User.Identity.Name;

            var result = await _blogService.CreateBlogPost(blogPost);
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