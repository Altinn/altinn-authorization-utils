using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

/// <summary>
/// Configures <see cref="AltinnPlatformAccessTokenSettings"/> from an <see cref="AltinnServiceDescriptor"/> and
/// <see cref="IConfiguration"/>.
/// </summary>
internal sealed class ConfigureAltinnPlatformAccessTokenSettingsFromConfiguration
    : IConfigureOptions<AltinnPlatformAccessTokenSettings>
    , IConfigureNamedOptions<AltinnPlatformAccessTokenSettings>
{
    private readonly AltinnServiceDescriptor _serviceDescriptor;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureAltinnPlatformAccessTokenSettingsFromConfiguration"/> class.
    /// </summary>
    public ConfigureAltinnPlatformAccessTokenSettingsFromConfiguration(
        AltinnServiceDescriptor serviceDescriptor,
        IConfiguration configuration)
    {
        _serviceDescriptor = serviceDescriptor;
        _configuration = configuration;
    }

    /// <inheritdoc/>
    public void Configure(AltinnPlatformAccessTokenSettings options)
        => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);

    /// <inheritdoc/>
    public void Configure(string? name, AltinnPlatformAccessTokenSettings options)
    {
        if (string.IsNullOrEmpty(options.AppName))
        {
            options.AppName = _serviceDescriptor.Name;
        }

        if (string.IsNullOrEmpty(options.Issuer))
        {
            options.Issuer = _configuration.GetValue("Platform:Token:Generator:Issuer", "platform");
        }
    }
}
