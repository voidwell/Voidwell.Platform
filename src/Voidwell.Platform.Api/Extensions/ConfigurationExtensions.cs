using System.Reflection;

namespace Voidwell.Platform.Api.Extensions;

internal static class ConfigurationExtensions
{
    public static string GetApplicationName(this IConfiguration configuration)
    {
        var value = configuration.GetValue<string?>("ApplicationName", null);
        if (string.IsNullOrWhiteSpace(value))
        {
            value = Assembly.GetExecutingAssembly().GetName().Name!;
        }
        return value;
    }
}
