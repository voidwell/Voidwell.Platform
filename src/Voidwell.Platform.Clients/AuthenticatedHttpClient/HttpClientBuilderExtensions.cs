using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Voidwell.Platform.Clients.AuthenticatedHttpClient;

public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Authenticates the client with client-credentials bearer tokens.
    /// </summary>
    public static IHttpClientBuilder AddTokenHandler<TClient>(this IHttpClientBuilder builder, Action<IServiceProvider, AuthenticatedHttpClientOptions> configure)
        where TClient : class
    {
        var services = builder.Services;

        builder.Services.AddTransient<IConfigureOptions<AuthenticatedHttpClientOptions>>(services =>
            {
                return new ConfigureNamedOptions<AuthenticatedHttpClientOptions>(builder.Name, (options) =>
                {
                    configure(services, options);
                });
            });

        services.TryAddSingleton(TimeProvider.System);

        if (!services.Any(d => d.ServiceType == typeof(IClientTokenService)))
        {
            services.AddHttpClient<IClientTokenService, ClientTokenService>();
        }

        services.TryAddSingleton<ITokenManager<TClient>, TokenManager<TClient>>();
        services.TryAddTransient<AuthenticatedHttpMessageHandler<TClient>>();

        return builder.AddHttpMessageHandler<AuthenticatedHttpMessageHandler<TClient>>();
    }
}
