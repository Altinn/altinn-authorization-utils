using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.MaskinPorten.Tests;

public class MaskinPortenConfigurationReloadTests
{
    private const string OriginalKeyJson = """{"kty":"oct","k":"dGVzdA","alg":"HS256","kid":"old-key"}""";
    private const string UpdatedKeyJson = """{"kty":"oct","k":"bmV3LXRlc3Q","alg":"HS256","kid":"new-key"}""";

    [Fact]
    public void ConfigurationReload_UpdatesCommonAndClientOptions_WhenOnlySharedValuesChange()
    {
        using var configuration = (ConfigurationRoot)new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Altinn:MaskinPorten:Endpoint"] = "https://old.example.com/",
                ["Altinn:MaskinPorten:Audience"] = "old-audience",
                ["Altinn:MaskinPorten:TokenDuration"] = "00:02:00",
                ["Altinn:MaskinPorten:Clients:test-client:ClientId"] = "test-client-id",
                ["Altinn:MaskinPorten:Clients:test-client:Scope"] = "test-scope",
                ["Altinn:MaskinPorten:Clients:test-client:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(OriginalKeyJson)),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddMaskinPortenClient();
        using var provider = services.BuildServiceProvider();

        var common = provider.GetRequiredService<IOptionsMonitor<MaskinPortenCommonOptions>>();
        var clients = provider.GetRequiredService<IOptionsMonitor<MaskinPortenClientOptions>>();
        var originalCommon = common.CurrentValue;
        var originalClient = clients.Get("test-client");

        configuration["Altinn:MaskinPorten:Endpoint"] = "https://new.example.com/";
        configuration["Altinn:MaskinPorten:Audience"] = "new-audience";
        configuration["Altinn:MaskinPorten:TokenDuration"] = "00:05:00";
        configuration.Reload();

        common.CurrentValue.ShouldSatisfyAllConditions(
            options => options.ShouldNotBeSameAs(originalCommon),
            options => options.Endpoint.ShouldBe(new Uri("https://new.example.com/")),
            options => options.Audience.ShouldBe("new-audience"),
            options => options.TokenDuration.ShouldBe(TimeSpan.FromMinutes(5)));

        clients.Get("test-client").ShouldSatisfyAllConditions(
            options => options.ShouldNotBeSameAs(originalClient),
            options => options.Endpoint.ShouldBe(new Uri("https://new.example.com/")),
            options => options.Audience.ShouldBe("new-audience"),
            options => options.TokenDuration.ShouldBe(TimeSpan.FromMinutes(5)),
            options => options.Scope.ShouldBe("test-scope"));
    }

    [Fact]
    public void ConfigurationReload_UpdatesClientSigningKey_WhenOnlyKeyChanges()
    {
        using var configuration = (ConfigurationRoot)new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Altinn:MaskinPorten:Clients:test-client:ClientId"] = "test-client-id",
                ["Altinn:MaskinPorten:Clients:test-client:Scope"] = "test-scope",
                ["Altinn:MaskinPorten:Clients:test-client:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(OriginalKeyJson)),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddMaskinPortenClient();
        using var provider = services.BuildServiceProvider();

        var clients = provider.GetRequiredService<IOptionsMonitor<MaskinPortenClientOptions>>();
        var originalClient = clients.Get("test-client");
        originalClient.Key.ShouldNotBeNull().ShouldSatisfyAllConditions(
            key => key.Kid.ShouldBe("old-key"),
            key => key.K.ShouldBe("dGVzdA"));

        configuration["Altinn:MaskinPorten:Clients:test-client:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(UpdatedKeyJson));
        configuration.Reload();

        var updatedClient = clients.Get("test-client");
        updatedClient.Key.ShouldNotBeNull().ShouldSatisfyAllConditions(
            key => key.Kid.ShouldBe("new-key"),
            key => key.K.ShouldBe("bmV3LXRlc3Q"));
        updatedClient.ShouldNotBeSameAs(originalClient);
    }
}
