using Serilog;
using Serilog.Formatting.Compact;

namespace Voidwell.Platform.Api.Extensions;

internal static class LoggingBuilderExtensions
{
    public static IHostApplicationBuilder AddApplicationLogging(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var hostEnvironment = builder.Environment;

        var applicationName = configuration.GetApplicationName();
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName);

        if (hostEnvironment.IsDevelopment())
        {
            loggerConfig.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][{SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}");
        }
        else
        {
            loggerConfig.WriteTo.Console(new CompactJsonFormatter());
        }

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(loggerConfig.CreateLogger());

        return builder;
    }
}
