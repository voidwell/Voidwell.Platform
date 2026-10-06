using System.Net.Http.Headers;

namespace Voidwell.Platform.Clients.AuthenticatedHttpClient;

/// <summary>Adds the bearer token to outgoing requests</summary>
internal sealed class AuthenticatedHttpMessageHandler<TClient> : DelegatingHandler
    where TClient : class
{
    private readonly ITokenManager<TClient> _tokenManager;

    public AuthenticatedHttpMessageHandler(ITokenManager<TClient> tokenManager)
    {
        _tokenManager = tokenManager;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SetAuthAndSendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SetAuthAndSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenManager.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
