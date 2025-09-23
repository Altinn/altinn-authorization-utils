namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// Specifies the identifier for an Altinn environment.
/// </summary>
public enum AltinnEnvironmentId
    : byte
{
    /// <summary>
    /// Generic unknown environment.
    /// </summary>
    /// <remarks>
    /// This is the default value and is explicitly invalid.
    /// </remarks>
    Unknown = default,

    /// <summary>
    /// AT21 acceptance test environment.
    /// </summary>
    AT21,

    /// <summary>
    /// AT22 acceptance test environment.
    /// </summary>
    AT22,

    /// <summary>
    /// AT23 acceptance test environment.
    /// </summary>
    AT23,

    /// <summary>
    /// AT24 acceptance test environment.
    /// </summary>
    AT24,

    /// <summary>
    /// YT01 performance test environment.
    /// </summary>
    YT01,

    /// <summary>
    /// TT02 staging environment.
    /// </summary>
    TT02,

    /// <summary>
    /// Production environment.
    /// </summary>
    PROD,
}
