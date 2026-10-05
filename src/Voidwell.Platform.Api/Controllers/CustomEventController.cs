using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voidwell.Platform.Api.Authentication;
using Voidwell.Platform.Api.Services;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Api.Controllers;

[Route("gameevent")]
public class CustomEventController : Controller
{
    private readonly ICustomEventService _customEventService;

    public CustomEventController(ICustomEventService customEventService)
    {
        _customEventService = customEventService;
    }

    [HttpGet("{eventId}")]
    public async Task<ActionResult> GetCustomEventById(int eventId)
    {
        var result = await _customEventService.GetCustomEventAsync(eventId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllCustomEvents()
    {
        var result = await _customEventService.GetAllCustomEventsAsync();
        return Ok(result);
    }

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult> GetAllCustomEventsByGame(string gameId)
    {
        var result = await _customEventService.GetAllCustomEventsByGameIdAsync(gameId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AuthConstants.Roles.Events)]
    public async Task<ActionResult> PostCustomEvent([FromBody] CustomEvent customEvent)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _customEventService.CreateCustomEventAsync(customEvent);
        return Created("gameevent", result);
    }

    [HttpPut("{eventId}")]
    [Authorize(Roles = AuthConstants.Roles.Events)]
    public async Task<ActionResult> PutCustomEvent(int eventId, [FromBody] CustomEvent customEvent)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _customEventService.UpdateCustomEventAsync(eventId, customEvent);
        return Ok(result);
    }

    [HttpDelete("{eventId}")]
    [Authorize(Roles = AuthConstants.Roles.Administrator)]
    public async Task<ActionResult> DeleteCustomEvent(int eventId)
    {
        await _customEventService.DeleteCustomEventAsync(eventId);
        return NoContent();
    }
}
