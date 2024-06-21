namespace Altinn.Authorization.JwkGenerator;

/// <summary>
/// JWK use.
/// </summary>
public enum JWKUse
{
    /// <summary>
    /// Key is used for signing.
    /// </summary>
    Sig,

    /// <summary>
    /// Key is used for encryption.
    /// </summary>
    Enc,
}
