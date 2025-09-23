using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

/// <summary>
/// Configures <see cref="AltinnTestTokenGeneratorSettings"/> from an <see cref="AltinnServiceDescriptor"/> and
/// <see cref="IConfiguration"/>.
/// </summary>
internal sealed class ConfigureAltinnTestTokenGeneratorSettingsFromConfiguration
    : IConfigureOptions<AltinnTestTokenGeneratorSettings>
    , IConfigureNamedOptions<AltinnTestTokenGeneratorSettings>
{
    private readonly AltinnServiceDescriptor _serviceDescriptor;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureAltinnTestTokenGeneratorSettingsFromConfiguration"/> class.
    /// </summary>
    public ConfigureAltinnTestTokenGeneratorSettingsFromConfiguration(
        AltinnServiceDescriptor serviceDescriptor,
        IConfiguration configuration)
    {
        _serviceDescriptor = serviceDescriptor;
        _configuration = configuration;
    }

    /// <inheritdoc/>
    public void Configure(AltinnTestTokenGeneratorSettings options)
        => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);

    /// <inheritdoc/>
    public void Configure(string? name, AltinnTestTokenGeneratorSettings options)
    {
        if (string.IsNullOrEmpty(options.AppName))
        {
            options.AppName = _serviceDescriptor.Name;
        }

        if (string.IsNullOrEmpty(options.EnvName))
        {
            options.EnvName = _serviceDescriptor.Environment.ToString("p");
        }

        if (string.IsNullOrEmpty(options.Url)
            && _configuration.GetValue<string?>("Platform:Token:TestTool:Endpoint", defaultValue: null) is { } url)
        {
            options.Url = url;
        }

        if (options.Authentication is null
            && _configuration.GetValue<string?>("Platform:Token:TestTool:Username", defaultValue: null) is { } username
            && _configuration.GetValue<string?>("Platform:Token:TestTool:Password", defaultValue: null) is { } password)
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            options.Authentication = new("Basic", credentials);
        }
    }
}
