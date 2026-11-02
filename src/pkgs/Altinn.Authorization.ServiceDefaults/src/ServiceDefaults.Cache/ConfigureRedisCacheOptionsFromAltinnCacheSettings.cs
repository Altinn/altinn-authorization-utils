using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Configures <see cref="RedisCacheOptions"/> from <see cref="AltinnCacheSettings"/>.
/// </summary>
internal sealed class ConfigureRedisCacheOptionsFromAltinnCacheSettings
    : IConfigureNamedOptions<RedisCacheOptions>
{
    private readonly IOptionsMonitor<AltinnCacheSettings> _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureRedisCacheOptionsFromAltinnCacheSettings"/> class.
    /// </summary>
    public ConfigureRedisCacheOptionsFromAltinnCacheSettings(IOptionsMonitor<AltinnCacheSettings> settings)
    {
        _settings = settings;
    }

    /// <inheritdoc/>
    public void Configure(RedisCacheOptions options)
        => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);

    /// <inheritdoc/>
    public void Configure(string? name, RedisCacheOptions options)
    {
        options.ConfigurationOptions = _settings.CurrentValue.Valkey.CreateConfigurationOptions();
    }
}
