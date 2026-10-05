using FluentAssertions;
using Voidwell.Platform.Api.Controllers;
using Xunit;

namespace Voidwell.Platform.Api.Test.Controllers;

public class UtilsControllerTest
{
    [Fact]
    public void GetServerTime_ReturnsCurrentUtcTime()
    {
        using var controller = new UtilsController();
        var before = DateTime.UtcNow;

        var result = controller.GetServerTime();

        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }
}
