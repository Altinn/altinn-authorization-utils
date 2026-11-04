using Altinn.Authorization.ServiceDefaults.Cache;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Altinn cache <see cref="IServiceCollection"/> extensions.
/// </summary>
public static class AltinnServiceDefaultsCacheServiceCollectionExtensions
{
    /// <summary>
    /// The largest value (in bytes) that is written to the distributed tier. Larger values stay in the local tier only.
    /// </summary>
    public const int MaximumPayloadBytes = 1024 * 1024;

    /// <summary>
    /// How long a value lives in the distributed tier.
    /// </summary>
    public static readonly TimeSpan DistributedCacheExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Registers <see cref="HybridCache"/> with the local (in-memory) tier only, and <see cref="AltinnCacheEntryOptions"/>
    /// for it. Calling this more than once is safe.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddAltinnCache(this IServiceCollection services)
    {
        Guard.IsNotNull(services);

        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = MaximumPayloadBytes;
        });
        services.TryAddSingleton(new AltinnCacheEntryOptions(localCacheExpiration: null));

        return services;
    }

    /// <summary>
    /// Registers <see cref="HybridCache"/> with the local (in-memory) tier always on, and Valkey as the distributed tier
    /// when <paramref name="enabled"/> is <see langword="true"/> and <c>Altinn:Cache:Host</c>, <c>Altinn:Cache:Username</c>
    /// and either <c>Altinn:Cache:Password</c> (local development) or <c>Altinn:Cache:PasswordFile</c> (Kubernetes) are
    /// configured. Otherwise the cache is local only. The active mode is logged once at startup.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    /// <param name="serviceName">The service name, used as the key prefix in the distributed tier.</param>
    /// <param name="enabled">Whether the distributed tier should be used (the app's own feature flag).</param>
    /// <param name="keyVersion">
    /// The key version, included in the key prefix as <c>{serviceName}:v{keyVersion}:</c>. Bump it when the shape of a
    /// cached type changes, so entries written by the previous version are not read.
    /// </param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddAltinnCache(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        bool enabled,
        int keyVersion = 1)
    {
        Guard.IsNotNull(services);
        Guard.IsNotNull(configuration);
        Guard.IsNotNullOrWhiteSpace(serviceName);
        Guard.IsGreaterThanOrEqualTo(keyVersion, 1);

        services.AddAltinnCache();

        var settings = configuration.GetSection(AltinnCacheSettings.SectionName).Get<AltinnCacheSettings>() ?? new();
        AltinnCacheMode mode;
        if (!enabled)
        {
            mode = new("local tier only (distributed tier not enabled)");
        }
        else if (string.IsNullOrWhiteSpace(settings.Host)
            || string.IsNullOrWhiteSpace(settings.Username)
            || (string.IsNullOrWhiteSpace(settings.Password) && string.IsNullOrWhiteSpace(settings.PasswordFile)))
        {
            mode = new($"local tier only ({AltinnCacheSettings.SectionName}:Host, Username, and Password or PasswordFile not configured)");
        }
        else if (string.IsNullOrWhiteSpace(settings.Password) && !File.Exists(settings.PasswordFile))
        {
            mode = new($"local tier only (password file '{settings.PasswordFile}' not found)");
        }
        else
        {
            var prefix = $"{serviceName}:v{keyVersion}:";
            var connectionOptions = AltinnCacheConnection.CreateOptions(settings);
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = prefix;
                options.ConnectionMultiplexerFactory = () => AltinnCacheConnection.ConnectAsync(connectionOptions, settings);
            });

            if (string.IsNullOrWhiteSpace(settings.Password))
            {
                // RedisCache keeps the multiplexer for the lifetime of the app, so a rotated password can only reach
                // it through the live connection options, which the multiplexer reads on every handshake.
                services.AddSingleton<IHostedService>(s => new AltinnCachePasswordRefresher(
                    connectionOptions,
                    settings,
                    s.GetRequiredService<ILogger<AltinnCachePasswordRefresher>>()));
            }

            services.Configure<HybridCacheOptions>(options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = DistributedCacheExpiration,
                    LocalCacheExpiration = settings.LocalCacheExpiration,
                };
            });
            services.Replace(ServiceDescriptor.Singleton(new AltinnCacheEntryOptions(settings.LocalCacheExpiration)));

            mode = new($"local tier and distributed tier at {settings.Host} as user {settings.Username}, key prefix '{prefix}'");
        }

        services.TryAddSingleton(mode);
        services.AddHostedService<AltinnCacheModeLogger>();

        return services;
    }
}
