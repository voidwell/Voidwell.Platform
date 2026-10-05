using Microsoft.AspNetCore.Mvc;

namespace Voidwell.Platform.Api.Controllers;

[Route("utils")]
public class UtilsController : Controller
{
    [HttpGet("time")]
    public DateTime GetServerTime()
    {
        return DateTime.UtcNow;
    }
}
