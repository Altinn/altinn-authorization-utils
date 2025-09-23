using Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;
using Microsoft.Extensions.Configuration;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class ConfigureAltinnPlatformAccessTokenSettingsFromConfigurationTests
{
    [Fact]
    public void DoesNot_Overwrite()
    {
        var config = Config([
            new("Platform:Token:Generator:Issuer", "test-issuer"),
        ]);

        var descriptor = new AltinnServiceDescriptor("test-app", AltinnEnvironment.Create("TEST"));
        var options = new AltinnPlatformAccessTokenSettings
        {
            AppName = "custom-app",
            Issuer = "custom-issuer",
        };

        var sut = new ConfigureAltinnPlatformAccessTokenSettingsFromConfiguration(descriptor, config);
        sut.Configure(options);

        options.AppName.ShouldBe("custom-app");
        options.Issuer.ShouldBe("custom-issuer");
    }

    [Fact]
    public void Sets_Values()
    {
        var config = Config([
            new("Platform:Token:Generator:Issuer", "test-issuer"),
        ]);

        var descriptor = new AltinnServiceDescriptor("test-app", AltinnEnvironment.Create("TEST"));
        var options = new AltinnPlatformAccessTokenSettings
        {
        };

        var sut = new ConfigureAltinnPlatformAccessTokenSettingsFromConfiguration(descriptor, config);
        sut.Configure(options);

        options.AppName.ShouldBe("test-app");
        options.Issuer.ShouldBe("test-issuer");
    }

    [Fact]
    public void Sets_Values_DefaultIssuer()
    {
        var config = Config([
        ]);

        var descriptor = new AltinnServiceDescriptor("test-app", AltinnEnvironment.Create("TEST"));
        var options = new AltinnPlatformAccessTokenSettings
        {
        };

        var sut = new ConfigureAltinnPlatformAccessTokenSettingsFromConfiguration(descriptor, config);
        sut.Configure(options);

        options.AppName.ShouldBe("test-app");
        options.Issuer.ShouldBe("platform");
    }

    private IConfigurationRoot Config(IEnumerable<KeyValuePair<string, string?>> pairs)
    {
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(pairs);
        return builder.Build();
    }
}
