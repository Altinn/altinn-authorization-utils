using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.ServiceDefaults;
using Altinn.Authorization.ServiceDefaults.Cache;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Host builder extensions for hybrid caching.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AltinnServiceDefaultsCacheServiceCollectionExtensions
{
    /// <param name="builder">The host application builder.</param>
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers <see cref="HybridCache"/> using the configured <see cref="AltinnServiceDescriptor"/> service name.
        /// The local tier is always available; the distributed Valkey tier is registered when enabled in
        /// <c>Altinn:Cache:Valkey</c>.
        /// </summary>
        /// <param name="configure">An optional callback to customize the cache settings after binding configuration.</param>
        /// <returns>An <see cref="IHybridCacheBuilder"/> for further cache configuration.</returns>
        /// <exception cref="InvalidOperationException">No Altinn service descriptor is registered.</exception>
        public IHybridCacheBuilder AddAltinnCache(
            Action<AltinnCacheSettings>? configure = null)
        {
            Guard.IsNotNull(builder);

            var serviceDescriptor = builder.Services.GetAltinnServiceDescriptor();

            return AddAltinnCache(builder, serviceDescriptor.Name, configure);
        }

        /// <summary>
        /// Registers <see cref="HybridCache"/> with settings from <c>Altinn:Cache</c> and the specified service name.
        /// The local tier is always available; the distributed Valkey tier is registered when enabled in
        /// <c>Altinn:Cache:Valkey</c>. The selected mode is reported by the pre-start logger.
        /// </summary>
        /// <param name="serviceName">The service name included in the distributed cache key prefix.</param>
        /// <param name="configure">An optional callback to customize the cache settings after binding configuration.</param>
        /// <returns>An <see cref="IHybridCacheBuilder"/> for further cache configuration.</returns>
        /// <exception cref="ArgumentException"><paramref name="serviceName"/> is empty or whitespace.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceName"/> is null.</exception>
        public IHybridCacheBuilder AddAltinnCache(
            string serviceName,
            Action<AltinnCacheSettings>? configure = null)
        {
            Guard.IsNotNull(builder);
            Guard.IsNotNullOrWhiteSpace(serviceName);

            var services = builder.Services;
            var logger = AltinnPreStartLogger.Create(builder.Configuration, nameof(AltinnServiceDefaultsCacheServiceCollectionExtensions));

            AltinnCacheSettings settings = new();
            builder.Configuration.GetSection(AltinnCacheSettings.SectionName).Bind(settings);
            configure?.Invoke(settings);

            services.AddOptions<AltinnCacheSettings>()
                .BindConfiguration(AltinnCacheSettings.SectionName)
                .Configure(options => configure?.Invoke(options))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var cacheBuilder = services.AddHybridCache();
            services.AddSingleton<IConfigureOptions<HybridCacheOptions>, ConfigureHybridCacheOptionsFromAltinnCacheSettings>();

            if (settings.Valkey is { Enabled: true })
            {
                var instanceName = $"{serviceName}:{settings.Valkey.KeyNamespaceVersion}:";
                logger.Log($"Configuring Valkey distributed cache with instance name '{instanceName}'");
                services.AddStackExchangeRedisCache(o => o.InstanceName = instanceName);
                services.AddSingleton<IConfigureOptions<RedisCacheOptions>, ConfigureRedisCacheOptionsFromAltinnCacheSettings>();
            }
            else
            {
                logger.Log("Configuring local (in-memory) cache only");
            }

            return cacheBuilder;
        }
    }
}
