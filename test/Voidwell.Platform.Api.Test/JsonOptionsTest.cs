using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Voidwell.Platform.Api.Extensions;
using Xunit;

namespace Voidwell.Platform.Api.Test;

public class JsonOptionsTest
{
    private sealed class Sample
    {
        public string Name { get; set; }
        public string Missing { get; set; }
        public DateTime When { get; set; }
        public Sample Self { get; set; }
    }

    [Fact]
    public void ApiJsonOptions_UseCamelCaseAndOmitNulls()
    {
        var options = GetJsonOptions();

        var json = JsonSerializer.Serialize(new Sample { Name = "x" }, options);

        json.Should().Contain("\"name\":\"x\"").And.NotContain("missing");
    }

    [Fact]
    public void ApiJsonOptions_IgnoreReferenceCycles()
    {
        var options = GetJsonOptions();
        var sample = new Sample { Name = "x" };
        sample.Self = sample;

        var act = () => JsonSerializer.Serialize(sample, options);

        act.Should().NotThrow();
    }

    [Fact]
    public void ApiJsonOptions_WriteDateTimesAsUtc()
    {
        var options = GetJsonOptions();
        var when = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc);

        var json = JsonSerializer.Serialize(new Sample { When = when }, options);

        json.Should().Contain("\"when\":\"2024-05-06T07:08:09Z\"");
    }

    [Fact]
    public void ApiJsonOptions_ReadDateTimesAsUtc()
    {
        var options = GetJsonOptions();

        var sample = JsonSerializer.Deserialize<Sample>("""{"when":"2024-05-06T07:08:09Z"}""", options);

        sample.When.Kind.Should().Be(DateTimeKind.Utc);
        sample.When.Should().Be(new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc));
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        var services = new ServiceCollection();
        services.AddControllers().AddApiJsonOptions();

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
    }
}
