using Microsoft.AspNetCore.Mvc;
using System;

namespace Voidwell.Internal.Controllers
{
    [Route("utils")]
    public class UtilsController : Controller
    {
        [HttpGet("time")]
        public DateTime GetServerTime()
        {
            return DateTime.UtcNow;
        }
    }
}
