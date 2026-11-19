using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.MaskinPorten.Options;

/// <summary>
/// Configures <see cref="MaskinPortenClientOptions"/> with default values from <see cref="MaskinPortenCommonOptions"/>.
/// </summary>
internal sealed class ConfigureMaskinPortenClientOptionsFromCommonOptions
    : IConfigureNamedOptions<MaskinPortenClientOptions>
    , IDisposable
{
    private readonly IOptionsMonitor<MaskinPortenCommonOptions> _common;
    private readonly IDisposable? _subscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureMaskinPortenClientOptionsFromCommonOptions"/> class.
    /// </summary>
    public ConfigureMaskinPortenClientOptionsFromCommonOptions(
        IOptionsMonitor<MaskinPortenCommonOptions> common,
        IOptionsMonitorCache<MaskinPortenClientOptions> cache)
    {
        _common = common;
        _subscription = common.OnChange((_, name) =>
        {
            if (string.Equals(name, Microsoft.Extensions.Options.Options.DefaultName, StringComparison.Ordinal))
            {
                cache.Clear();
            }
        });
    }

    /// <inheritdoc/>
    public void Configure(string? name, MaskinPortenClientOptions options)
    {
        Configure(options, _common.CurrentValue);
    }

    /// <inheritdoc/>
    public void Configure(MaskinPortenClientOptions options)
    {
        Configure(options, _common.CurrentValue);
    }

    /// <inheritdoc/>
    public void Dispose()
        => _subscription?.Dispose();

    private void Configure(MaskinPortenClientOptions options, MaskinPortenCommonOptions common)
    {
        options.Endpoint ??= common.Endpoint;
        options.Audience ??= common.Audience;
        options.TokenDuration ??= common.TokenDuration;
    }
}
