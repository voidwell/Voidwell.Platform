using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Voidwell.Internal.Services
{
    public class UserHelper : IUserHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetUserId()
        {
            var claims = _httpContextAccessor.HttpContext.User?.Claims;
            var subjectClaim = claims?.FirstOrDefault(c => c.Type == "sub")?.Value;

            Guid userId;
            if (!Guid.TryParse(subjectClaim, out userId))
            {
                return null;
            }

            return userId;
        }
    }
}
