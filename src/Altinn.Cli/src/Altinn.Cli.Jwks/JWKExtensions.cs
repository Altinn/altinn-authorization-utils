using CommunityToolkit.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Altinn.Authorization.JwkGenerator;

[ExcludeFromCodeCoverage]
internal static class JWKExtensions
{
    public static string ToJwkString(this JWKAlgorithm alg)
        => alg switch
        {
            JWKAlgorithm.RS256 => SecurityAlgorithms.RsaSha256,
            JWKAlgorithm.RS384 => SecurityAlgorithms.RsaSha384,
            JWKAlgorithm.RS512 => SecurityAlgorithms.RsaSha512,
            JWKAlgorithm.ES256 => SecurityAlgorithms.EcdsaSha256,
            JWKAlgorithm.ES384 => SecurityAlgorithms.EcdsaSha384,
            JWKAlgorithm.ES512 => SecurityAlgorithms.EcdsaSha512,
            _ => ThrowHelper.ThrowArgumentException<string>(nameof(alg), "Unknown JWK Algorithm"),
        };

    public static string ToJwkString(this JWKUse use)
        => use switch
        {
            JWKUse.sig => JsonWebKeyUseNames.Sig,
            JWKUse.enc => JsonWebKeyUseNames.Enc,
            _ => ThrowHelper.ThrowArgumentException<string>(nameof(use), "Unknown JWK Use"),
        };

    public static ECCurve ToECDsaCurve(this JWKAlgorithm alg)
        => alg switch
        {
            JWKAlgorithm.ES256 => ECCurve.NamedCurves.nistP256,
            JWKAlgorithm.ES384 => ECCurve.NamedCurves.nistP384,
            JWKAlgorithm.ES512 => ECCurve.NamedCurves.nistP521,
            _ => ThrowHelper.ThrowArgumentException<ECCurve>(nameof(alg), "Unknown JWK Algorithm"),
        };
}
