using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

internal enum JsonWebKeySetEnvironment
{
    /// <summary>
    /// Test keys available.
    /// </summary>
    Test = 1 << 0,
    
    /// <summary>
    /// Production keys available.
    /// </summary>
    Prod = 1 << 1,
}

[ExcludeFromCodeCoverage]
internal static class JsonWebKeySetEnvironmentExtensions
{
    public static string Name(this JsonWebKeySetEnvironment environment)
        => environment switch
        {
            JsonWebKeySetEnvironment.Test => "TEST",
            JsonWebKeySetEnvironment.Prod => "PROD",
            _ => throw new ArgumentOutOfRangeException(nameof(environment)),
        };
}

[Flags]
internal enum JsonWebKeySetEnvironments
{
    /// <summary>
    /// No keys available.
    /// </summary>
    None = 0,

    /// <summary>
    /// Test keys available.
    /// </summary>
    Test = 1 << 0,

    /// <summary>
    /// Production keys available.
    /// </summary>
    Prod = 1 << 1,
}

/// <summary>
/// JWKS variants available.
/// </summary>
[Flags]
public enum JsonWebKeySetEnvironmentFilter
{
    /// <summary>
    /// No keys available.
    /// </summary>
    None = 0,

    /// <summary>
    /// Test keys available.
    /// </summary>
    Test = 1 << 0,

    /// <summary>
    /// Production keys available.
    /// </summary>
    Prod = 1 << 1,

    /// <summary>
    /// All keys.
    /// </summary>
    All = ~None,
}
