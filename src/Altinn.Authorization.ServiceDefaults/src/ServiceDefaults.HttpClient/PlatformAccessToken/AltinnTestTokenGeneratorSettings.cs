using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

/// <summary>
/// Settings for using the test token generator.
/// </summary>
public sealed class AltinnTestTokenGeneratorSettings
{
    /// <summary>
    /// The URL of the test token generator.
    /// </summary>
    [Required]
    public string? Url { get; set; }

    /// <summary>
    /// The environment name sent to the test token generator.
    /// </summary>
    [Required]
    public string? EnvName { get; set; }

    /// <summary>
    /// The application name sent to the test token generator.
    /// </summary>
    [Required]
    public string? AppName { get; set; }

    /// <summary>
    /// The authentication header value to use when calling the test token generator.
    /// </summary>
    [Required]
    public AuthenticationHeaderValue? Authentication { get; set; }
}
