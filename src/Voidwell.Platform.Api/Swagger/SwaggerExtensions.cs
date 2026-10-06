using Microsoft.OpenApi;
using Voidwell.Platform.Api.Extensions;

namespace Voidwell.Platform.Api.Swagger;

public static class SwaggerExtensions
{
    private const string _bearerScheme = "Bearer";

    public static IServiceCollection AddApiSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationName = configuration.GetApplicationName();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = applicationName,
                Version = "v1"
            });

            // Authentication is optional: the scheme is only defined here and is applied per operation
            // by AuthorizeOperationFilter, so anonymous endpoints can be called without a token.
            options.AddSecurityDefinition(_bearerScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Access token for endpoints that require authorization. Not needed for public endpoints."
            });

            options.OperationFilter<AuthorizeOperationFilter>(_bearerScheme);
        });

        return services;
    }

    public static WebApplication UseApiSwagger(this WebApplication app)
    {
        var applicationName = app.Configuration.GetApplicationName();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{applicationName} v1");
        });

        return app;
    }
}
