using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Configures <see cref="HybridCacheOptions"/> from <see cref="AltinnCacheSettings"/>.
/// </summary>
internal sealed class ConfigureHybridCacheOptionsFromAltinnCacheSettings
    : IConfigureNamedOptions<HybridCacheOptions>
{
    private readonly IOptionsMonitor<AltinnCacheSettings> _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureHybridCacheOptionsFromAltinnCacheSettings"/> class.
    /// </summary>
    public ConfigureHybridCacheOptionsFromAltinnCacheSettings(IOptionsMonitor<AltinnCacheSettings> settings)
    {
        _settings = settings;
    }

    /// <inheritdoc/>
    public void Configure(HybridCacheOptions options)
        => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);

    /// <inheritdoc/>
    public void Configure(string? name, HybridCacheOptions options)
    {
        var settings = _settings.CurrentValue;
        var hasDistributedCache = settings.Valkey.Enabled;
        var localExpiration = hasDistributedCache
            ? settings.DefaultLocalCacheExpirationWithDistributedCache
            : settings.DefaultLocalCacheExpiration;
        options.MaximumPayloadBytes = settings.MaximumPayloadBytes;
        options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = hasDistributedCache
                ? settings.DefaultDistributedCacheExpiration
                : localExpiration,
            LocalCacheExpiration = localExpiration,
            Flags = options.DefaultEntryOptions?.Flags,
        };
    }
}
