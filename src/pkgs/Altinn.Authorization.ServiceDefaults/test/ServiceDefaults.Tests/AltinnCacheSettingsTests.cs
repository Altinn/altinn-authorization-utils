using Altinn.Authorization.ServiceDefaults.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class AltinnCacheSettingsTests
{
    [Fact]
    public void Unconfigured_UsesDefaultsWithoutRequiringValkeyCredentials()
    {
        using var provider = CreateProvider([]);

        var settings = provider.GetRequiredService<IOptions<AltinnCacheSettings>>().Value;

        settings.Valkey.Enabled.ShouldBeFalse();
        settings.Valkey.KeyNamespaceVersion.ShouldBe((ushort)1);
        settings.Valkey.Host.ShouldBeNull();
        settings.Valkey.Username.ShouldBeNull();
        settings.Valkey.Password.ShouldBeNull();
        settings.MaximumPayloadBytes.ShouldBe(1024 * 1024);
        settings.DefaultDistributedCacheExpiration.ShouldBe(TimeSpan.FromMinutes(5));
        settings.DefaultLocalCacheExpiration.ShouldBe(TimeSpan.FromMinutes(5));
        settings.DefaultLocalCacheExpirationWithDistributedCache.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Configured_BindsAndValidatesNestedSettings()
    {
        using var provider = CreateProvider(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = "true",
            ["Altinn:Cache:Valkey:Host"] = "valkey:6379",
            ["Altinn:Cache:Valkey:Username"] = "app",
            ["Altinn:Cache:Valkey:Password"] = "password",
            ["Altinn:Cache:Valkey:KeyNamespaceVersion"] = "65535",
            ["Altinn:Cache:MaximumPayloadBytes"] = "2048",
            ["Altinn:Cache:DefaultDistributedCacheExpiration"] = "00:10:00",
            ["Altinn:Cache:DefaultLocalCacheExpiration"] = "00:00:15",
            ["Altinn:Cache:DefaultLocalCacheExpirationWithDistributedCache"] = "00:00:20",
        });

        var settings = provider.GetRequiredService<IOptions<AltinnCacheSettings>>().Value;

        settings.Valkey.Enabled.ShouldBeTrue();
        settings.Valkey.KeyNamespaceVersion.ShouldBe(ushort.MaxValue);
        settings.Valkey.Host.ShouldBe("valkey:6379");
        settings.Valkey.Username.ShouldBe("app");
        settings.Valkey.Password.ShouldBe("password");
        settings.MaximumPayloadBytes.ShouldBe(2048);
        settings.DefaultDistributedCacheExpiration.ShouldBe(TimeSpan.FromMinutes(10));
        settings.DefaultLocalCacheExpiration.ShouldBe(TimeSpan.FromSeconds(15));
        settings.DefaultLocalCacheExpirationWithDistributedCache.ShouldBe(TimeSpan.FromSeconds(20));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public void Disabled_DoesNotValidateCredentials(string? value)
    {
        using var provider = CreateProvider(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = "false",
            ["Altinn:Cache:Valkey:Host"] = value,
            ["Altinn:Cache:Valkey:Username"] = value,
            ["Altinn:Cache:Valkey:Password"] = value,
        });

        provider.GetRequiredService<IOptions<AltinnCacheSettings>>().Value.Valkey.Enabled.ShouldBeFalse();
    }

    [Theory]
    [InlineData(nameof(AltinnValkeyCacheSettings.Host), null)]
    [InlineData(nameof(AltinnValkeyCacheSettings.Host), "")]
    [InlineData(nameof(AltinnValkeyCacheSettings.Host), " \t")]
    [InlineData(nameof(AltinnValkeyCacheSettings.Username), null)]
    [InlineData(nameof(AltinnValkeyCacheSettings.Username), "")]
    [InlineData(nameof(AltinnValkeyCacheSettings.Username), " \t")]
    [InlineData(nameof(AltinnValkeyCacheSettings.Password), null)]
    [InlineData(nameof(AltinnValkeyCacheSettings.Password), "")]
    [InlineData(nameof(AltinnValkeyCacheSettings.Password), " \t")]
    public void Enabled_RejectsMissingCredential(string member, string? value)
    {
        var configuration = new Dictionary<string, string?>
        {
            ["Altinn:Cache:Valkey:Enabled"] = "true",
            ["Altinn:Cache:Valkey:Host"] = "valkey:6379",
            ["Altinn:Cache:Valkey:Username"] = "app",
            ["Altinn:Cache:Valkey:Password"] = "password",
        };
        configuration[$"Altinn:Cache:Valkey:{member}"] = value;
        using var provider = CreateProvider(configuration);

        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<AltinnCacheSettings>>().Value);

        var failure = exception.Failures.ShouldHaveSingleItem();
        failure.ShouldContain("AltinnCacheSettings.Valkey");
        failure.ShouldContain($"{member} is required when Valkey is enabled");
    }

    [Fact]
    public void EnabledWithoutCredentials_ReportsAllMissingMembers()
    {
        using var provider = CreateProvider(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = "true",
        });

        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<AltinnCacheSettings>>().Value);

        exception.Failures.Count().ShouldBe(3);
        exception.Failures.ShouldContain(f => f.Contains("Host is required"));
        exception.Failures.ShouldContain(f => f.Contains("Username is required"));
        exception.Failures.ShouldContain(f => f.Contains("Password is required"));
    }

    [Theory]
    [InlineData(nameof(AltinnCacheSettings.DefaultDistributedCacheExpiration), "-00:00:01")]
    [InlineData(nameof(AltinnCacheSettings.DefaultDistributedCacheExpiration), "00:00:00")]
    [InlineData(nameof(AltinnCacheSettings.DefaultDistributedCacheExpiration), "00:00:59.9999999")]
    [InlineData(nameof(AltinnCacheSettings.DefaultLocalCacheExpiration), "-00:00:01")]
    [InlineData(nameof(AltinnCacheSettings.DefaultLocalCacheExpiration), "00:00:00")]
    [InlineData(nameof(AltinnCacheSettings.DefaultLocalCacheExpiration), "00:00:09.9999999")]
    [InlineData(nameof(AltinnCacheSettings.DefaultLocalCacheExpirationWithDistributedCache), "-00:00:01")]
    [InlineData(nameof(AltinnCacheSettings.DefaultLocalCacheExpirationWithDistributedCache), "00:00:00")]
    [InlineData(nameof(AltinnCacheSettings.DefaultLocalCacheExpirationWithDistributedCache), "00:00:09.9999999")]
    public void ExpirationBelowMinimum_IsRejected(string member, string value)
    {
        using var provider = CreateProvider(new()
        {
            [$"Altinn:Cache:{member}"] = value,
        });

        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<AltinnCacheSettings>>().Value);

        exception.Failures.ShouldHaveSingleItem().ShouldContain(member);
    }

    [Theory]
    [InlineData("00:01:00", "00:00:10")]
    [InlineData("00:01:00.0000001", "00:00:10.0000001")]
    public void ExpirationsAtOrAboveMinimum_AreAccepted(string distributed, string local)
    {
        using var provider = CreateProvider(new()
        {
            ["Altinn:Cache:DefaultDistributedCacheExpiration"] = distributed,
            ["Altinn:Cache:DefaultLocalCacheExpiration"] = local,
            ["Altinn:Cache:DefaultLocalCacheExpirationWithDistributedCache"] = local,
        });

        var settings = provider.GetRequiredService<IOptions<AltinnCacheSettings>>().Value;

        settings.DefaultDistributedCacheExpiration.ShouldBe(TimeSpan.Parse(distributed));
        settings.DefaultLocalCacheExpiration.ShouldBe(TimeSpan.Parse(local));
        settings.DefaultLocalCacheExpirationWithDistributedCache.ShouldBe(TimeSpan.Parse(local));
    }

    [Fact]
    public void InvalidParentAndNestedSettings_ReportsAllFailures()
    {
        using var provider = CreateProvider(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = "true",
            ["Altinn:Cache:DefaultDistributedCacheExpiration"] = "00:00:00",
            ["Altinn:Cache:DefaultLocalCacheExpiration"] = "00:00:00",
            ["Altinn:Cache:DefaultLocalCacheExpirationWithDistributedCache"] = "00:00:00",
        });

        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<AltinnCacheSettings>>().Value);

        exception.Failures.Count().ShouldBe(6);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZeroKeyVersion_IsRejectedOnlyWhenEnabled(bool enabled)
    {
        using var provider = CreateProvider(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = enabled.ToString(),
            ["Altinn:Cache:Valkey:Host"] = "valkey:6379",
            ["Altinn:Cache:Valkey:Username"] = "app",
            ["Altinn:Cache:Valkey:Password"] = "password",
            ["Altinn:Cache:Valkey:KeyNamespaceVersion"] = "0",
        });
        var options = provider.GetRequiredService<IOptions<AltinnCacheSettings>>();

        if (enabled)
        {
            var exception = Should.Throw<OptionsValidationException>(() => options.Value);
            exception.Failures.ShouldHaveSingleItem().ShouldContain(nameof(AltinnValkeyCacheSettings.KeyNamespaceVersion));
        }
        else
        {
            options.Value.Valkey.KeyNamespaceVersion.ShouldBe((ushort)0);
        }
    }

    [Theory]
    [InlineData(false, 300, 300)]
    [InlineData(true, 300, 30)]
    public void HybridCache_DefaultExpirationsDependOnDistributedTier(bool enabled, int expirationSeconds, int localSeconds)
    {
        using var provider = CreateHybridCacheProvider(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = enabled.ToString(),
        });

        var options = provider.GetRequiredService<IOptions<HybridCacheOptions>>().Value.DefaultEntryOptions.ShouldNotBeNull();

        options.Expiration.ShouldBe(TimeSpan.FromSeconds(expirationSeconds));
        options.LocalCacheExpiration.ShouldBe(TimeSpan.FromSeconds(localSeconds));
    }

    [Theory]
    [InlineData(false, 600, 600)]
    [InlineData(true, 120, 15)]
    public void HybridCache_CustomExpirationsDependOnDistributedTier(bool enabled, int expirationSeconds, int localSeconds)
    {
        using var provider = CreateHybridCacheProvider(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = enabled.ToString(),
            ["Altinn:Cache:DefaultDistributedCacheExpiration"] = "00:02:00",
            ["Altinn:Cache:DefaultLocalCacheExpiration"] = "00:10:00",
            ["Altinn:Cache:DefaultLocalCacheExpirationWithDistributedCache"] = "00:00:15",
        });

        var options = provider.GetRequiredService<IOptions<HybridCacheOptions>>().Value.DefaultEntryOptions.ShouldNotBeNull();

        options.Expiration.ShouldBe(TimeSpan.FromSeconds(expirationSeconds));
        options.LocalCacheExpiration.ShouldBe(TimeSpan.FromSeconds(localSeconds));
    }

    private static ServiceProvider CreateHybridCacheProvider(Dictionary<string, string?> values)
    {
        values["Altinn:Cache:Valkey:Host"] = "valkey:6379";
        values["Altinn:Cache:Valkey:Username"] = "app";
        values["Altinn:Cache:Valkey:Password"] = "password";
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(values);
        builder.AddAltinnCache("settings-test");

        return builder.Services.BuildServiceProvider();
    }

    private static ServiceProvider CreateProvider(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddOptions<AltinnCacheSettings>()
            .Bind(configuration.GetSection(AltinnCacheSettings.SectionName))
            .ValidateDataAnnotations();

        return services.BuildServiceProvider();
    }
}
