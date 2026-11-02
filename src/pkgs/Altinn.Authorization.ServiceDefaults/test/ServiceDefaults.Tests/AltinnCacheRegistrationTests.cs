using Altinn.Authorization.ServiceDefaults.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class AltinnCacheRegistrationTests
{
    [Fact]
    public async Task Unconfigured_UsesLocalCacheWithoutHostedServices()
    {
        var builder = CreateBuilder();
        builder.AddAltinnCache("test");
        using var host = builder.Build();
        await host.StartAsync(TestContext.Current.CancellationToken);

        host.Services.GetService<IDistributedCache>().ShouldBeNull();
        host.Services.GetServices<IHostedService>().ShouldBeEmpty();
        var cache = host.Services.GetRequiredService<HybridCache>();
        await cache.SetAsync("key", "value", cancellationToken: TestContext.Current.CancellationToken);
        var value = await cache.GetOrCreateAsync("key", _ => ValueTask.FromResult("fresh"), cancellationToken: TestContext.Current.CancellationToken);
        value.ShouldBe("value");

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void Callback_OverridesConfigurationForRegistrationAndRuntimeOptions()
    {
        var builder = CreateBuilder(new()
        {
            ["Altinn:Cache:MaximumPayloadBytes"] = "1024",
            ["Altinn:Cache:Valkey:Enabled"] = "false",
            ["Altinn:Cache:Valkey:Host"] = "old-host:6379",
        });
        builder.AddAltinnCache("callback", settings =>
        {
            settings.MaximumPayloadBytes = 4096;
            settings.DefaultDistributedCacheExpiration = TimeSpan.FromMinutes(10);
            settings.DefaultLocalCacheExpirationWithDistributedCache = TimeSpan.FromSeconds(45);
            settings.Valkey.Enabled = true;
            settings.Valkey.Host = "valkey:6380";
            settings.Valkey.Username = "app";
            settings.Valkey.Password = "password";
            settings.Valkey.KeyNamespaceVersion = 12;
        });
        using var host = builder.Build();

        var hybrid = host.Services.GetRequiredService<IOptions<HybridCacheOptions>>().Value;
        hybrid.MaximumPayloadBytes.ShouldBe(4096);
        hybrid.DefaultEntryOptions.ShouldNotBeNull();
        hybrid.DefaultEntryOptions.Expiration.ShouldBe(TimeSpan.FromMinutes(10));
        hybrid.DefaultEntryOptions.LocalCacheExpiration.ShouldBe(TimeSpan.FromSeconds(45));
        host.Services.GetRequiredService<IDistributedCache>().ShouldBeAssignableTo<RedisCache>();
        var redis = host.Services.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
        redis.InstanceName.ShouldBe("callback:12:");
        var connection = redis.ConfigurationOptions.ShouldNotBeNull();
        connection.EndPoints.ShouldHaveSingleItem().ShouldBe(new System.Net.DnsEndPoint("valkey", 6380));
        connection.User.ShouldBe("app");
        connection.Password.ShouldBe("password");
        connection.ConnectTimeout.ShouldBe(200);
        connection.SyncTimeout.ShouldBe(200);
        connection.AsyncTimeout.ShouldBe(200);
        connection.AbortOnConnectFail.ShouldBeFalse();
        connection.Ssl.ShouldBeFalse();
        connection.BacklogPolicy.ShouldBeSameAs(BacklogPolicy.FailFast);
    }

    [Fact]
    public void Callback_CanDisableConfiguredValkey()
    {
        var builder = CreateBuilder(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = "true",
        });
        builder.AddAltinnCache("test", settings => settings.Valkey.Enabled = false);
        using var host = builder.Build();

        host.Services.GetRequiredService<IOptions<AltinnCacheSettings>>().Value.Valkey.Enabled.ShouldBeFalse();
        host.Services.GetService<IDistributedCache>().ShouldBeNull();
        host.Services.GetRequiredService<IOptions<HybridCacheOptions>>().Value.DefaultEntryOptions
            .ShouldNotBeNull().LocalCacheExpiration.ShouldBe(TimeSpan.FromMinutes(5));
    }

    [Theory]
    [InlineData("Altinn:Cache:Valkey:Enabled", "true")]
    [InlineData("Altinn:Cache:DefaultDistributedCacheExpiration", "00:00:59")]
    [InlineData("Altinn:Cache:DefaultLocalCacheExpiration", "00:00:09")]
    [InlineData("Altinn:Cache:DefaultLocalCacheExpirationWithDistributedCache", "00:00:09")]
    public async Task InvalidConfiguration_FailsAtHostStartup(string key, string value)
    {
        var builder = CreateBuilder(new() { [key] = value });
        builder.AddAltinnCache("test");
        using var host = builder.Build();

        await Should.ThrowAsync<OptionsValidationException>(() => host.StartAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task InvalidCallback_FailsAtHostStartup()
    {
        var builder = CreateBuilder();
        builder.AddAltinnCache("test", settings => settings.DefaultLocalCacheExpiration = TimeSpan.Zero);
        using var host = builder.Build();

        await Should.ThrowAsync<OptionsValidationException>(() => host.StartAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void DescriptorOverload_UsesRegisteredServiceName()
    {
        var builder = CreateBuilder(new()
        {
            ["Altinn:Cache:Valkey:Enabled"] = "true",
            ["Altinn:Cache:Valkey:Host"] = "valkey:6379",
            ["Altinn:Cache:Valkey:Username"] = "app",
            ["Altinn:Cache:Valkey:Password"] = "password",
        });
        builder.Services.AddSingleton(new AltinnServiceDescriptor("descriptor-service", AltinnEnvironment.Create("TEST"), flags: default));
        builder.AddAltinnCache();
        using var host = builder.Build();

        host.Services.GetRequiredService<IOptions<RedisCacheOptions>>().Value.InstanceName.ShouldBe("descriptor-service:1:");
    }

    [Fact]
    public void DescriptorOverload_RequiresRegisteredServiceDescriptor()
    {
        var builder = CreateBuilder();

        Should.Throw<InvalidOperationException>(() => builder.AddAltinnCache());
    }

    private static HostApplicationBuilder CreateBuilder(Dictionary<string, string?>? values = null)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [AltinnPreStartLogger.DisableConfigKey] = "true",
        });
        if (values is not null)
        {
            builder.Configuration.AddInMemoryCollection(values);
        }

        return builder;
    }
}
