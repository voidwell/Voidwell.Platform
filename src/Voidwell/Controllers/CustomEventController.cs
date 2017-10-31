using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Voidwell.Models;
using Voidwell.Services;

namespace Voidwell.Controllers
{
    [Route("gameevent")]
    public class CustomEventController : Controller
    {
        private readonly ICustomEventService _customEventService;

        public CustomEventController(ICustomEventService customEventService)
        {
            _customEventService = customEventService;
        }

        [HttpGet("{eventId}")]
        public async Task<ActionResult> GetCustomEventById(string eventId)
        {
            var result = await _customEventService.GetCustomEvent(eventId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllCustomEvents()
        {
            var result = await _customEventService.GetAllCustomEvents();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> PostCustomEvent([FromBody] CustomEvent customEvent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _customEventService.CreateCustomEvent(customEvent);
            return Created("gameevent", result);
        }

        [HttpPut("{eventId}")]
        public async Task<ActionResult> PutCustomEvent(string eventId, [FromBody] CustomEvent customEvent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _customEventService.UpdateCustomEvent(eventId, customEvent);
            return Ok(result);
        }

        [HttpDelete("{eventId}")]
        public async Task<ActionResult> DeleteCustomEvent(string eventId)
        {
            await _customEventService.DeleteCustomEvent(eventId);
            return NoContent();
        }
    }
}
