using Altinn.Authorization.ServiceDefaults.HttpClient;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class AsyncOnlyDelegatingHandlerTests
{
    [Fact]
    public void Send_ThrowsNotSupportedException()
    {
        var handler = new TestHandler();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        using var client = new System.Net.Http.HttpClient(handler);
        var exception = Should.Throw<NotSupportedException>(() => client.Send(request, CancellationToken.None));
    }

    public class TestHandler 
        : AsyncOnlyDelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            return Task.FromResult(response);
        }
    }
}
