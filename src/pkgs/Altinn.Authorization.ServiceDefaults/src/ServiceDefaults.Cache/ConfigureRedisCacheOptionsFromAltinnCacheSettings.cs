using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Configures <see cref="RedisCacheOptions"/> from <see cref="AltinnCacheSettings"/>.
/// </summary>
internal sealed class ConfigureRedisCacheOptionsFromAltinnCacheSettings
    : IConfigureNamedOptions<RedisCacheOptions>
    , IDisposable
{
    private readonly IOptionsMonitor<AltinnCacheSettings> _settings;
    private readonly IDisposable? _subscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureRedisCacheOptionsFromAltinnCacheSettings"/> class.
    /// </summary>
    public ConfigureRedisCacheOptionsFromAltinnCacheSettings(
        IOptionsMonitor<AltinnCacheSettings> settings,
        IOptionsMonitorCache<RedisCacheOptions> cache)
    {
        _settings = settings;
        _subscription = settings.OnChange((_, name) =>
        {
            if (string.Equals(name, Microsoft.Extensions.Options.Options.DefaultName, StringComparison.Ordinal))
            {
                cache.Clear();
            }
        });
    }

    /// <inheritdoc/>
    public void Dispose()
        => _subscription?.Dispose();

    /// <inheritdoc/>
    public void Configure(RedisCacheOptions options)
        => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);

    /// <inheritdoc/>
    public void Configure(string? name, RedisCacheOptions options)
    {
        options.ConfigurationOptions = _settings.CurrentValue.Valkey.CreateConfigurationOptions();
    }
}
