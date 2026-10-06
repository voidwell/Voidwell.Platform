namespace Voidwell.Platform.Clients.AuthenticatedHttpClient;

public interface ITokenManager<TClient>
    where TClient : class
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);

    void Invalidate(string rejectedToken);
}
