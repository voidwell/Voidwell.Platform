namespace Voidwell.Platform.Clients.AuthenticatedHttpClient;

internal interface IClientTokenService
{
    Task<TokenResponse> RequestTokenAsync<TClient>(CancellationToken cancellationToken);
}
