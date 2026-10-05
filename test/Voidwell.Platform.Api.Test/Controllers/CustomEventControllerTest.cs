using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Voidwell.Platform.Api.Controllers;
using Voidwell.Platform.Api.Models;
using Voidwell.Platform.Api.Services;
using Voidwell.Platform.Data.Models;
using Xunit;

namespace Voidwell.Platform.Api.Test.Controllers;

public sealed class CustomEventControllerTest : IDisposable
{
    private readonly Mock<ICustomEventService> _service = new();
    private readonly CustomEventController _subject;

    public CustomEventControllerTest()
    {
        _subject = new CustomEventController(_service.Object);
    }

    public void Dispose()
    {
        _subject.Dispose();
    }

    [Fact]
    public async Task GetCustomEventById_ReturnsDetails()
    {
        var details = new CustomEventDetails { Id = 4 };
        _service.Setup(a => a.GetCustomEventAsync(4)).ReturnsAsync(details);

        var result = await _subject.GetCustomEventById(4);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(details);
    }

    [Fact]
    public async Task GetAllCustomEvents_ReturnsEvents()
    {
        var events = new[] { new CustomEvent { Id = 1 } };
        _service.Setup(a => a.GetAllCustomEventsAsync()).ReturnsAsync(events);

        var result = await _subject.GetAllCustomEvents();

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(events);
    }

    [Fact]
    public async Task GetAllCustomEventsByGame_ReturnsEventsForGame()
    {
        var events = new[] { new CustomEvent { Id = 1, GameId = "game" } };
        _service.Setup(a => a.GetAllCustomEventsByGameIdAsync("game")).ReturnsAsync(events);

        var result = await _subject.GetAllCustomEventsByGame("game");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(events);
    }

    [Fact]
    public async Task PostCustomEvent_ReturnsCreated()
    {
        var customEvent = new CustomEvent { Name = "Event" };
        var created = new CustomEvent { Id = 9, Name = "Event" };
        _service.Setup(a => a.CreateCustomEventAsync(customEvent)).ReturnsAsync(created);

        var result = await _subject.PostCustomEvent(customEvent);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be("gameevent");
        createdResult.Value.Should().BeSameAs(created);
    }

    [Fact]
    public async Task PostCustomEvent_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        _subject.ModelState.AddModelError("Name", "Required");

        var result = await _subject.PostCustomEvent(new CustomEvent());

        result.Should().BeOfType<BadRequestObjectResult>();
        _service.Verify(a => a.CreateCustomEventAsync(It.IsAny<CustomEvent>()), Times.Never);
    }

    [Fact]
    public async Task PutCustomEvent_ReturnsUpdatedEvent()
    {
        var customEvent = new CustomEvent { Id = 2, Name = "Event" };
        _service.Setup(a => a.UpdateCustomEventAsync(2, customEvent)).ReturnsAsync(customEvent);

        var result = await _subject.PutCustomEvent(2, customEvent);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(customEvent);
    }

    [Fact]
    public async Task PutCustomEvent_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        _subject.ModelState.AddModelError("Name", "Required");

        var result = await _subject.PutCustomEvent(2, new CustomEvent());

        result.Should().BeOfType<BadRequestObjectResult>();
        _service.Verify(a => a.UpdateCustomEventAsync(It.IsAny<int>(), It.IsAny<CustomEvent>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCustomEvent_DeletesAndReturnsNoContent()
    {
        var result = await _subject.DeleteCustomEvent(2);

        result.Should().BeOfType<NoContentResult>();
        _service.Verify(a => a.DeleteCustomEventAsync(2), Times.Once);
    }
}
