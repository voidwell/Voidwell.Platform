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
    public async Task<ActionResult> GetCustomEventById(int eventId, CancellationToken cancellationToken)
    {
        var result = await _customEventService.GetCustomEventAsync(eventId, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllCustomEvents(CancellationToken cancellationToken)
    {
        var result = await _customEventService.GetCustomEventsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult> GetAllCustomEventsByGame(string gameId, CancellationToken cancellationToken)
    {
        var result = await _customEventService.GetCustomEventsAsync(gameId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AuthConstants.Roles.Events)]
    public async Task<ActionResult> PostCustomEvent([FromBody] CustomEvent customEvent, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _customEventService.CreateCustomEventAsync(customEvent, cancellationToken);
        return Created("gameevent", result);
    }

    [HttpPut("{eventId}")]
    [Authorize(Roles = AuthConstants.Roles.Events)]
    public async Task<ActionResult> PutCustomEvent(int eventId, [FromBody] CustomEvent customEvent, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _customEventService.UpdateCustomEventAsync(eventId, customEvent, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{eventId}")]
    [Authorize(Roles = AuthConstants.Roles.Administrator)]
    public async Task<ActionResult> DeleteCustomEvent(int eventId, CancellationToken cancellationToken)
    {
        await _customEventService.DeleteCustomEventAsync(eventId, cancellationToken);
        return NoContent();
    }
}
