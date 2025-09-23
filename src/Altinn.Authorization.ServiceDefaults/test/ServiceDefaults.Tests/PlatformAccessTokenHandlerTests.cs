using Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;
using System.Net;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class PlatformAccessTokenHandlerTests
{
    [Fact]
    public async Task DoesNotAddToken_IfTokenIsSet()
    {
        var handler = new PlatformAccessTokenHandler(new TestPlatformAccessTokenProvider("test-token"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Add("PlatformAccessToken", "existing");

        var token = await TestHandler(handler, request);

        token.ShouldBe("existing");
    }

    [Fact]
    public async Task DoesNotAddToken_IfExplicitlyDisabled()
    {
        var handler = new PlatformAccessTokenHandler(new TestPlatformAccessTokenProvider("test-token"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.DisablePlatformAccessToken();

        var token = await TestHandler(handler, request);

        token.ShouldBe(null);
    }

    [Fact]
    public async Task AddsToken()
    {
        var handler = new PlatformAccessTokenHandler(new TestPlatformAccessTokenProvider("test-token"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        var token = await TestHandler(handler, request);

        token.ShouldBe("test-token");
    }

    private async Task<string?> TestHandler(PlatformAccessTokenHandler handler, HttpRequestMessage request)
    {
        handler.InnerHandler = new TestInnerHandler();

        using var client = new System.Net.Http.HttpClient(handler);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        if (content == "$$NULL$$")
        {
            return null;
        }

        return content;
    }

    private class TestPlatformAccessTokenProvider(string token)
        : IPlatformAccessTokenProvider
    {
        public ValueTask<string> GetPlatformAccessToken(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(token);
        }
    }

    private class TestInnerHandler
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = request.Headers.TryGetValues("PlatformAccessToken", out var values) ? string.Join(",", values) : "$$NULL$$";
            return Task.FromResult(new HttpResponseMessage() { RequestMessage = request, StatusCode = HttpStatusCode.OK, Content = new StringContent(token) });
        }
    }
}
