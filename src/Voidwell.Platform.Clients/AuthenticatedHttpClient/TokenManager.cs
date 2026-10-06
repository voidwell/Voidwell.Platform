namespace Voidwell.Platform.Clients.AuthenticatedHttpClient;

internal sealed class TokenManager<TClient> : ITokenManager<TClient>
    where TClient : class
{
    private static readonly TimeSpan _expiryBuffer = TimeSpan.FromSeconds(30);

    private readonly TimeProvider _timeProvider;
    private readonly IClientTokenService _clientTokenService;

    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _accessToken;
    private DateTimeOffset? _expiration;

    public TokenManager(TimeProvider timeProvider, IClientTokenService clientTokenService)
    {
        _timeProvider = timeProvider;
        _clientTokenService = clientTokenService;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (TryGetCachedToken(out var cached))
        {
            return cached;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Another caller may have refreshed the token while we waited.
            if (TryGetCachedToken(out cached))
            {
                return cached;
            }

            var response = await _clientTokenService.RequestTokenAsync<TClient>(cancellationToken);

            _accessToken = response.AccessToken;
            _expiration = _timeProvider.GetUtcNow().AddSeconds(response.ExpiresIn);

            return response.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private bool TryGetCachedToken(out string token)
    {
        if (_accessToken is not null && _expiration is not null && _timeProvider.GetUtcNow() < _expiration - _expiryBuffer)
        {
            token = _accessToken;
            return true;
        }

        token = string.Empty;
        return false;
    }

    public void Invalidate(string rejectedToken)
    {
        _lock.Wait();
        try
        {
            // Only clear it if nobody has already replaced the rejected token.
            if (_accessToken == rejectedToken)
            {
                _accessToken = null;
                _expiration = null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}
