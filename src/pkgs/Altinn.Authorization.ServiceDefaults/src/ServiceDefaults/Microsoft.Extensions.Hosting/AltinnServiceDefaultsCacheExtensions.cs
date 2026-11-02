using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Host builder extensions for the Altinn cache.
/// </summary>
public static class AltinnServiceDefaultsCacheExtensions
{
    /// <summary>
    /// Registers <see cref="HybridCache"/> with the local (in-memory) tier always on, and Valkey as the distributed tier
    /// when <paramref name="enabled"/> is <see langword="true"/> and <c>Altinn:Cache</c> is configured.
    /// See <see cref="AltinnServiceDefaultsCacheServiceCollectionExtensions.AddAltinnCache(IServiceCollection, Configuration.IConfiguration, string, bool, int)"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <param name="enabled">Whether the distributed tier should be used (the app's own feature flag).</param>
    /// <param name="keyVersion">The key version; bump it when the shape of a cached type changes.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static IHostApplicationBuilder AddAltinnCache(this IHostApplicationBuilder builder, bool enabled, int keyVersion = 1)
    {
        Guard.IsNotNull(builder);

        var serviceDescriptor = builder.GetAltinnServiceDescriptor();
        builder.Services.AddAltinnCache(builder.Configuration, serviceDescriptor.Name, enabled, keyVersion);

        return builder;
    }
}
