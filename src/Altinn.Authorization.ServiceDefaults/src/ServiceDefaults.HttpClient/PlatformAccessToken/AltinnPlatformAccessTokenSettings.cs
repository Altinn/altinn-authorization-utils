using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

/// <summary>
/// Settings for the <see cref="AccessTokenClientTokenProvider"/>.
/// </summary>
public sealed class AltinnPlatformAccessTokenSettings
{
    /// <summary>
    /// The issuer to use when generating platform access tokens.
    /// </summary>
    [Required]
    public string? Issuer { get; set; }

    /// <summary>
    /// The application name to use when generating platform access tokens.
    /// </summary>
    [Required]
    public string? AppName { get; set; }
}
