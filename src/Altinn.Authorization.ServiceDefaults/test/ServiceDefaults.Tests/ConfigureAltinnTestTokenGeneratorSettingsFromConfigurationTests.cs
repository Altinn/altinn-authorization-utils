using Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;
using Altinn.Authorization.TestUtils.Http;
using Microsoft.Extensions.Configuration;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class ConfigureAltinnTestTokenGeneratorSettingsFromConfigurationTests
{
    [Fact]
    public void DoesNot_Overwrite()
    {
        var config = Config([
            new("Platform:Token:TestTool:Endpoint", FakeHttpMessageHandler.FakeBasePath.OriginalString),
            new("Platform:Token:TestTool:Username", "test-user"),
            new("Platform:Token:TestTool:Password", "test-pass"),
        ]);

        var descriptor = new AltinnServiceDescriptor("test-app", AltinnEnvironment.Create("TEST"));
        var options = new AltinnTestTokenGeneratorSettings
        {
            AppName = "custom-app",
            EnvName = "custom-env",
            Url = "https://custom-url/",
            Authentication = new("Custom", "auth"),
        };

        var sut = new ConfigureAltinnTestTokenGeneratorSettingsFromConfiguration(descriptor, config);
        sut.Configure(options);

        options.AppName.ShouldBe("custom-app");
        options.EnvName.ShouldBe("custom-env");
        options.Url.ShouldBe("https://custom-url/");
        options.Authentication.ShouldNotBeNull();
        options.Authentication.Scheme.ShouldBe("Custom");
        options.Authentication.Parameter.ShouldBe("auth");
    }

    [Fact]
    public void Sets_Values()
    {
        var config = Config([
            new("Platform:Token:TestTool:Endpoint", FakeHttpMessageHandler.FakeBasePath.OriginalString),
            new("Platform:Token:TestTool:Username", "test-user"),
            new("Platform:Token:TestTool:Password", "test-pass"),
        ]);

        var descriptor = new AltinnServiceDescriptor("test-app", AltinnEnvironment.Create("TEST"));
        var options = new AltinnTestTokenGeneratorSettings
        {
        };

        var sut = new ConfigureAltinnTestTokenGeneratorSettingsFromConfiguration(descriptor, config);
        sut.Configure(options);

        options.AppName.ShouldBe("test-app");
        options.EnvName.ShouldBe("test");
        options.Url.ShouldBe(FakeHttpMessageHandler.FakeBasePath.OriginalString);
        options.Authentication.ShouldNotBeNull();
        options.Authentication.Scheme.ShouldBe("Basic");
        options.Authentication.Parameter.ShouldBe("dGVzdC11c2VyOnRlc3QtcGFzcw==");
    }

    private IConfigurationRoot Config(IEnumerable<KeyValuePair<string, string?>> pairs)
    {
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(pairs);
        return builder.Build();
    }
}
