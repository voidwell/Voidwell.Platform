namespace Voidwell.Platform.Api.Services;

public class UserHelper : IUserHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        var claims = _httpContextAccessor.HttpContext?.User.Claims;
        var subjectClaim = claims?.FirstOrDefault(c => c.Type == "sub")?.Value;

        if (!Guid.TryParse(subjectClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
