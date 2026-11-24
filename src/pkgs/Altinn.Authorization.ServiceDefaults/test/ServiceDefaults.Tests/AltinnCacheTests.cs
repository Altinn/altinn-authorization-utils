using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Diagnostics;
using Testcontainers.Redis;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class AltinnCacheTests(ITestOutputHelper output)
    : IAsyncLifetime
{
    private const string User = "app";

    private readonly RedisContainer _valkey = new RedisBuilder("valkey/valkey:9.0-alpine3.24").Build();
    private ConnectionMultiplexer _admin = null!;

    public async ValueTask InitializeAsync()
    {
        await _valkey.StartAsync();

        // Like the platform's Valkey, require authentication: the container's default user is open, and
        // StackExchange.Redis silently falls back to it when AUTH fails.
        var bootstrap = await ConnectionMultiplexer.ConnectAsync($"{_valkey.GetConnectionString()},allowAdmin=true");
        await bootstrap.GetServer(bootstrap.GetEndPoints()[0]).ExecuteAsync("ACL", "SETUSER", "admin", "on", ">admin-password", "~*", "&*", "+@all");
        await bootstrap.DisposeAsync();
        _admin = await ConnectionMultiplexer.ConnectAsync($"{_valkey.GetConnectionString()},user=admin,password=admin-password,allowAdmin=true");
        await Server.ExecuteAsync("ACL", "SETUSER", "default", "off");

        await Server.ExecuteAsync("ACL", "SETUSER", User, "on", "resetpass", ">password-1", "~*", "&*", "+@all");
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await _admin.DisposeAsync();
        await _valkey.DisposeAsync();
    }

    [Fact]
    public async Task PasswordFromConfiguration_Authenticates()
    {
        await using var instance = CreateInstance();

        instance.GetServices<IHostedService>().ShouldBeEmpty();

        var distributedCache = instance.GetRequiredService<IDistributedCache>();
        await distributedCache.SetStringAsync("key", "value", TestContext.Current.CancellationToken);
        var value = await distributedCache.GetStringAsync("key", TestContext.Current.CancellationToken);

        value.ShouldBe("value");
    }

    [Fact]
    public async Task ValueSetByOneInstance_IsReadByAnother()
    {
        await using var instanceA = CreateInstance();
        await using var instanceB = CreateInstance();

        await instanceA.GetRequiredService<HybridCache>().SetAsync("key", "value", cancellationToken: TestContext.Current.CancellationToken);
        var value = await instanceB.GetRequiredService<HybridCache>().GetOrCreateAsync("key", _ => ValueTask.FromResult("fresh"), cancellationToken: TestContext.Current.CancellationToken);

        value.ShouldBe("value");
        Server.Keys(pattern: "test:1:*").ShouldNotBeEmpty();
    }

    [Fact]
    public async Task NamespaceVersions_DoNotCollideWhenKeysStartWithDigits()
    {
        await using var instanceA = CreateInstance(keyNamespaceVersion: 1);
        await using var instanceB = CreateInstance(keyNamespaceVersion: 12);
        var cacheA = instanceA.GetRequiredService<IDistributedCache>();
        var cacheB = instanceB.GetRequiredService<IDistributedCache>();

        await cacheA.SetStringAsync("23", "version-1", TestContext.Current.CancellationToken);
        await cacheB.SetStringAsync("3", "version-12", TestContext.Current.CancellationToken);

        (await cacheA.GetStringAsync("23", TestContext.Current.CancellationToken)).ShouldBe("version-1");
        (await cacheB.GetStringAsync("3", TestContext.Current.CancellationToken)).ShouldBe("version-12");
    }

    [Fact]
    public async Task RemoveOnOneInstance_IsSeenByAnother()
    {
        await using var instanceA = CreateInstance();
        await using var instanceB = CreateInstance();

        await instanceA.GetRequiredService<HybridCache>().SetAsync("key", "value", cancellationToken: TestContext.Current.CancellationToken);
        await instanceA.GetRequiredService<HybridCache>().RemoveAsync("key", cancellationToken: TestContext.Current.CancellationToken);
        var value = await instanceB.GetRequiredService<HybridCache>().GetOrCreateAsync("key", _ => ValueTask.FromResult("fresh"), cancellationToken: TestContext.Current.CancellationToken);

        value.ShouldBe("fresh");
    }

    [Fact]
    public async Task RemoveByTagOnOneInstance_IsSeenByAnother()
    {
        await using var instanceA = CreateInstance();
        await using var instanceB = CreateInstance();

        await instanceA.GetRequiredService<HybridCache>().SetAsync("key", "value", tags: ["tag"], cancellationToken: TestContext.Current.CancellationToken);
        await instanceA.GetRequiredService<HybridCache>().RemoveByTagAsync("tag", cancellationToken: TestContext.Current.CancellationToken);
        var value = await instanceB.GetRequiredService<HybridCache>().GetOrCreateAsync("key", _ => ValueTask.FromResult("fresh"), cancellationToken: TestContext.Current.CancellationToken);

        value.ShouldBe("fresh");
    }

    [Fact]
    public async Task EntryWrittenWithDefaults_LeavesLocalTierAfterLocalLifetime()
    {
        var localLifetime = TimeSpan.FromSeconds(10);
        await using var instance = CreateInstance(localCacheExpiration: localLifetime);
        var cache = instance.GetRequiredService<HybridCache>();
        var distributedCache = instance.GetRequiredService<IDistributedCache>();

        await cache.SetAsync("key", "value", cancellationToken: TestContext.Current.CancellationToken);

        await Task.Delay(localLifetime + TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        // Still in the distributed tier, so remove it there directly: a read must now miss both tiers.
        (await distributedCache.GetAsync("key", TestContext.Current.CancellationToken)).ShouldNotBeNull();
        await distributedCache.RemoveAsync("key", TestContext.Current.CancellationToken);
        var value = await cache.GetOrCreateAsync("key", _ => ValueTask.FromResult("fresh"), cancellationToken: TestContext.Current.CancellationToken);

        value.ShouldBe("fresh");
    }

    [Fact]
    public async Task UnreachableValkey_RequestsSucceed()
    {
        await using var instance = CreateInstance();
        var cache = instance.GetRequiredService<HybridCache>();
        await cache.SetAsync("key", "value", cancellationToken: TestContext.Current.CancellationToken);

        await _valkey.StopAsync(TestContext.Current.CancellationToken);

        var budget = TimeSpan.FromSeconds(3);
        for (var i = 0; i < 5; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var value = await cache.GetOrCreateAsync($"miss-{i}", _ => ValueTask.FromResult("fresh"), cancellationToken: TestContext.Current.CancellationToken);
            stopwatch.Stop();

            output.WriteLine($"miss {i}: {stopwatch.ElapsedMilliseconds} ms");
            value.ShouldBe("fresh");
            stopwatch.Elapsed.ShouldBeLessThan(budget);
        }
    }

    private IServer Server
        => _admin.GetServer(_admin.GetEndPoints()[0]);

    private ServiceProvider CreateInstance(TimeSpan? localCacheExpiration = null, ushort keyNamespaceVersion = 1)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Altinn:Cache:Valkey:Enabled"] = "true",
            ["Altinn:Cache:Valkey:Host"] = _valkey.GetConnectionString(),
            ["Altinn:Cache:Valkey:Username"] = User,
            ["Altinn:Cache:Valkey:Password"] = "password-1",
            ["Altinn:Cache:Valkey:KeyNamespaceVersion"] = keyNamespaceVersion.ToString(),
            ["Altinn:Cache:DefaultLocalCacheExpirationWithDistributedCache"] = localCacheExpiration?.ToString(),
            [AltinnPreStartLogger.DisableConfigKey] = "true",
        }.Where(setting => setting.Value is not null));
        builder.AddAltinnCache("test");

        return builder.Services.BuildServiceProvider();
    }
}
