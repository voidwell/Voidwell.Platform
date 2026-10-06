using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Voidwell.Common.Authentication;
using Voidwell.Platform.Clients.DaybreakGames;
using Voidwell.Platform.Clients.Keycloak;

namespace Voidwell.Platform.Clients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformClients(this IServiceCollection services)
    {
        services.AddDaybreakGamesClient();
        services.AddKeycloakClient();

        return services;
    }

    private static void AddDaybreakGamesClient(this IServiceCollection services)
    {
        services.AddOptions<DaybreakGamesOptions>()
            .BindConfiguration(DaybreakGamesOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IDaybreakGamesClient, DaybreakGamesClient>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IOptions<DaybreakGamesOptions>>().Value.BaseUrl;
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddTokenHandler((sp, o) =>
        {
            var source = sp.GetRequiredService<IOptions<DaybreakGamesOptions>>().Value;
            o.TokenServiceAddress = source.TokenServiceAddress;
            o.ClientId = source.ClientId;
            o.ClientSecret = source.ClientSecret;
            o.ClientScopes = ParseScopes(source.Scopes);
        });
    }

    private static void AddKeycloakClient(this IServiceCollection services)
    {
        services.AddOptions<KeycloakOptions>()
            .BindConfiguration(KeycloakOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IKeycloakClient, KeycloakClient>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IOptions<KeycloakOptions>>().Value.BaseUrl;
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddTokenHandler((sp, o) =>
        {
            var source = sp.GetRequiredService<IOptions<KeycloakOptions>>().Value;
            o.TokenServiceAddress = source.TokenServiceAddress;
            o.ClientId = source.ClientId;
            o.ClientSecret = source.ClientSecret;
            o.ClientScopes = ParseScopes(source.Scopes);
        });
    }

    private static List<string> ParseScopes(string scopes)
    {
        return [.. scopes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];
    }
}
