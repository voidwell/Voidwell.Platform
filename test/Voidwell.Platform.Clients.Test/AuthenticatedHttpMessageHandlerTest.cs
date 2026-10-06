using System.Net;
using FluentAssertions;
using Voidwell.Platform.Clients.AuthenticatedHttpClient;
using Xunit;

namespace Voidwell.Platform.Clients.Test;

public sealed class AuthenticatedHttpMessageHandlerTest : IDisposable
{
    private readonly RecordingHandler _inner = new();
    private readonly AuthenticatedHttpMessageHandler<AuthenticatedHttpMessageHandlerTest> _handler;
    private readonly HttpMessageInvoker _invoker;

    public AuthenticatedHttpMessageHandlerTest()
    {
        _handler = new AuthenticatedHttpMessageHandler<AuthenticatedHttpMessageHandlerTest>(new FakeTokenManager()) { InnerHandler = _inner };
        _invoker = new HttpMessageInvoker(_handler, disposeHandler: false);
    }

    public void Dispose()
    {
        _invoker.Dispose();
        _handler.Dispose();
        _inner.Dispose();
    }

    [Fact]
    public async Task SendAsync_AddsTheBearerToken()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://api.test/resource");

        using var response = await _invoker.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _inner.Authorizations.Should().Equal("Bearer token-1");
    }

    private sealed class FakeTokenManager : ITokenManager<AuthenticatedHttpMessageHandlerTest>
    {
        public Task<string> GetTokenAsync(CancellationToken cancellationToken = default) => Task.FromResult("token-1");

        public void Invalidate(string rejectedToken)
        {
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> Authorizations { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorizations.Add(request.Headers.Authorization?.ToString());

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
