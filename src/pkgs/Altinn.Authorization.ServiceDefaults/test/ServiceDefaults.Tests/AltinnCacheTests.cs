using Altinn.Authorization.ServiceDefaults.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Testing;
using StackExchange.Redis;
using System.Diagnostics;
using Testcontainers.Redis;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class AltinnCacheTests(ITestOutputHelper output)
    : IAsyncLifetime
{
    private const string User = "app";

    private readonly RedisContainer _valkey = new RedisBuilder("valkey/valkey:9.0-alpine3.24").Build();
    private readonly string _passwordFile = Path.GetTempFileName();
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

        await SetPassword("password-1");
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await _admin.DisposeAsync();
        await _valkey.DisposeAsync();
        File.Delete(_passwordFile);
    }

    [Fact]
    public async Task NotEnabled_IsLocalOnly()
    {
        await using var instance = CreateInstance(enabled: false);

        instance.GetService<IDistributedCache>().ShouldBeNull();

        foreach (var hostedService in instance.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(TestContext.Current.CancellationToken);
        }

        instance.GetRequiredService<FakeLogCollector>().GetSnapshot()
            .ShouldContain(r => r.Message == "Altinn cache: local tier only (distributed tier not enabled)");

        var cache = instance.GetRequiredService<HybridCache>();
        await cache.SetAsync("key", "value", cancellationToken: TestContext.Current.CancellationToken);
        var value = await cache.GetOrCreateAsync("key", _ => ValueTask.FromResult("fresh"), cancellationToken: TestContext.Current.CancellationToken);
        value.ShouldBe("value");
    }

    [Fact]
    public async Task PasswordFromConfiguration_NeedsNoFile()
    {
        await using var instance = CreateInstance(password: "password-1");

        // Nothing to re-read, so the password refresher (internal to the Cache project) is not registered.
        instance.GetServices<IHostedService>().ShouldNotContain(service => service.GetType().Name == "AltinnCachePasswordRefresher");

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
        Server.Keys(pattern: "test:v1:*").ShouldNotBeEmpty();
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
    public async Task EntryWrittenThroughOptions_LeavesLocalTierAfterLocalLifetime()
    {
        var localLifetime = TimeSpan.FromMilliseconds(200);
        await using var instance = CreateInstance(localCacheExpiration: localLifetime);
        var cache = instance.GetRequiredService<HybridCache>();
        var distributedCache = instance.GetRequiredService<IDistributedCache>();

        var options = instance.GetRequiredService<AltinnCacheEntryOptions>().For(TimeSpan.FromMinutes(5));
        options.LocalCacheExpiration.ShouldBe(localLifetime);
        await cache.SetAsync("key", "value", options, cancellationToken: TestContext.Current.CancellationToken);

        await Task.Delay(localLifetime * 2, TestContext.Current.CancellationToken);

        // Still in the distributed tier, so remove it there directly: a read must now miss both tiers.
        (await distributedCache.GetAsync("key", TestContext.Current.CancellationToken)).ShouldNotBeNull();
        await distributedCache.RemoveAsync("key", TestContext.Current.CancellationToken);
        var value = await cache.GetOrCreateAsync("key", _ => ValueTask.FromResult("fresh"), cancellationToken: TestContext.Current.CancellationToken);

        value.ShouldBe("fresh");
    }

    [Fact]
    public async Task RotatedPassword_IsUsedOnReconnect()
    {
        await using var instance = CreateInstance(passwordRefreshInterval: TimeSpan.FromMilliseconds(100));
        foreach (var hostedService in instance.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(TestContext.Current.CancellationToken);
        }

        var distributedCache = instance.GetRequiredService<IDistributedCache>();
        await distributedCache.SetStringAsync("key", "value", TestContext.Current.CancellationToken);

        // Rotate in Valkey only and drop the connection: reconnecting with the password the instance connected with fails.
        await Server.ExecuteAsync("ACL", "SETUSER", User, "resetpass", ">password-2");
        await Server.ExecuteAsync("CLIENT", "KILL", "USER", User);
        await Should.ThrowAsync<RedisConnectionException>(() => distributedCache.GetStringAsync("key", TestContext.Current.CancellationToken));

        // Rotate the file too: the refresher picks it up and the next reconnect uses it.
        await File.WriteAllTextAsync(_passwordFile, "password-2" + Environment.NewLine, TestContext.Current.CancellationToken);
        string? value = null;
        var attempts = 0;
        while (value is null && attempts++ < 50)
        {
            try
            {
                value = await distributedCache.GetStringAsync("key", TestContext.Current.CancellationToken);
            }
            catch (RedisConnectionException)
            {
                await Task.Delay(100, TestContext.Current.CancellationToken);
            }
        }

        output.WriteLine($"read succeeded on attempt {attempts} after the file was rotated");
        value.ShouldBe("value");
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

    private async Task SetPassword(string password)
    {
        await Server.ExecuteAsync("ACL", "SETUSER", User, "on", "resetpass", $">{password}", "~*", "&*", "+@all");
        await File.WriteAllTextAsync(_passwordFile, password + Environment.NewLine, TestContext.Current.CancellationToken);
    }

    private ServiceProvider CreateInstance(bool enabled = true, TimeSpan? passwordRefreshInterval = null, TimeSpan? localCacheExpiration = null, string? password = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Altinn:Cache:Host"] = _valkey.GetConnectionString(),
                ["Altinn:Cache:Username"] = User,
                ["Altinn:Cache:Password"] = password,
                ["Altinn:Cache:PasswordFile"] = password is null ? _passwordFile : null,
                ["Altinn:Cache:PasswordRefreshInterval"] = passwordRefreshInterval?.ToString(),
                ["Altinn:Cache:LocalCacheExpiration"] = localCacheExpiration?.ToString(),
            }.Where(setting => setting.Value is not null))
            .Build();

        var services = new ServiceCollection();
        services.AddFakeLogging();
        services.AddAltinnCache(configuration, "test", enabled);

        return services.BuildServiceProvider();
    }
}
