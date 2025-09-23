using Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;
using Altinn.Common.AccessTokenClient.Services;
using System.Security.Cryptography.X509Certificates;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class AccessTokenClientTokenProviderTests
{
    [Fact]
    public async Task GetPlatformAccessToken_Calls_GenerateToken()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AltinnPlatformAccessTokenSettings
        {
            Issuer = "test-issuer",
            AppName = "test-app",
        });

        var generator = new FakeAccessTokenGenerator();
        var sut = new AccessTokenClientTokenProvider(generator, options);

        var token = await sut.GetPlatformAccessToken(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), TestContext.Current.CancellationToken);
        token.ShouldBe("fake-token");

        generator.Issuer.ShouldBe("test-issuer");
        generator.AppName.ShouldBe("test-app");
    }

    private class FakeAccessTokenGenerator
        : IAccessTokenGenerator
    {
        public string? Issuer { get; private set; }

        public string? AppName { get; private set; }

        public string GenerateAccessToken(string issuer, string app)
        {
            Issuer = issuer;
            AppName = app;
            return "fake-token";
        }

        public string GenerateAccessToken(string issuer, string app, X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }
    }
}
