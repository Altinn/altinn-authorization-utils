using Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;
using Altinn.Authorization.TestUtils.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class TestTokenGeneratorPlatformAccessTokenProviderTests
{
    [Fact]
    public async Task GetPlatformAccessToken_Calls_TokenProvider()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AltinnTestTokenGeneratorSettings
        {
            Url = FakeHttpEndpoint.HttpsUri.OriginalString,
            AppName = "test-app",
            EnvName = "test-env",
            Authentication = new("Fake", "auth"),
        });

        using var handler = new FakeHttpMessageHandler();
        handler.Expect(HttpMethod.Get, "api/GetPlatformAccessToken")
            .WithQuery("env", "test-env")
            .WithQuery("app", "test-app")
            .WithQuery("ttl", "300")
            .WithAuthentication("Fake", "auth")
            .Respond("text/plain", "fake-token");

        using var client = handler.CreateClient();

        var sut = new TestTokenGeneratorPlatformAccessTokenProvider(
            client: client,
            options: options,
            logger: NullLogger<TestTokenGeneratorPlatformAccessTokenProvider>.Instance);

        var token = await sut.GetPlatformAccessToken(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), TestContext.Current.CancellationToken);
        token.ShouldBe("fake-token");
    }
}
