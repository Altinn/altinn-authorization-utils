namespace Altinn.Cli.Jwks;

/// <summary>
/// JWK use.
/// </summary>
public enum JsonWebKeyUse
{
    /// <summary>
    /// Key is used for signing.
    /// </summary>
    sig,

    /// <summary>
    /// Key is used for encryption.
    /// </summary>
    enc,
}
