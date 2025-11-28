namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// A set of well-known platform-access-token issuers.
/// </summary>
[Flags]
public enum WellKnownPlatformAccessTokenIssuers
    : uint
{
    /// <summary>
    /// Indicates that no options are set.
    /// </summary>
    None = default,

    /// <summary>
    /// Platform issuer.
    /// </summary>
    Platform = 1 << 0,
}
