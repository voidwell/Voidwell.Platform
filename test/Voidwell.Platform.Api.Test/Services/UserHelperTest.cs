using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Voidwell.Platform.Api.Services;
using Xunit;

namespace Voidwell.Platform.Api.Test.Services;

public class UserHelperTest
{
    [Fact]
    public void GetUserId_ReturnsSubjectClaimAsGuid()
    {
        var userId = Guid.NewGuid();

        var result = CreateSubject(new Claim("sub", userId.ToString())).GetUserId();

        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_ReturnsNull_WhenSubjectIsNotAGuid()
    {
        var result = CreateSubject(new Claim("sub", "not-a-guid")).GetUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserId_ReturnsNull_WhenThereIsNoSubjectClaim()
    {
        var result = CreateSubject(new Claim("name", "someone")).GetUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserId_ReturnsNull_WhenThereIsNoHttpContext()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(a => a.HttpContext).Returns((HttpContext)null);

        var result = new UserHelper(accessor.Object).GetUserId();

        result.Should().BeNull();
    }

    private static UserHelper CreateSubject(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(a => a.HttpContext).Returns(httpContext);

        return new UserHelper(accessor.Object);
    }
}
