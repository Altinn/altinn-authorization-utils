namespace Altinn.Cli.Jwks;

/// <summary>
/// JWK algorithm.
/// </summary>
public enum JsonWebKeyAlgorithm
{
    /// <summary>
    /// RSASSA-PKCS1-v1_5 using SHA-256
    /// </summary>
    /// <remarks>
    /// A key of size 2048 bits or larger MUST be used with these algorithms.
    /// </remarks>
    RS256,

    /// <summary>
    /// RSASSA-PKCS1-v1_5 using SHA-384
    /// </summary>
    /// <remarks>
    /// A key of size 2048 bits or larger MUST be used with these algorithms.
    /// </remarks>
    RS384,

    /// <summary>
    /// RSASSA-PKCS1-v1_5 using SHA-512
    /// </summary>
    /// <remarks>
    /// A key of size 2048 bits or larger MUST be used with these algorithms.
    /// </remarks>
    RS512,

    // TODO: ECDSA isn't really working at the moment, so it's disabled
    ///// <summary>
    ///// ECDSA using P-256 and SHA-256
    ///// </summary>
    //ES256,

    ///// <summary>
    ///// ECDSA using P-384 and SHA-384
    ///// </summary>
    //ES384,

    ///// <summary>
    ///// ECDSA using P-521 and SHA-512
    ///// </summary>
    //ES512,
}
