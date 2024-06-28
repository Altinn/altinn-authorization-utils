using CommunityToolkit.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class JsonWebKeyExtensions
{
    public static string ToJwkString(this JsonWebKeyAlgorithm alg)
        => alg switch
        {
            JsonWebKeyAlgorithm.RS256 => SecurityAlgorithms.RsaSha256,
            JsonWebKeyAlgorithm.RS384 => SecurityAlgorithms.RsaSha384,
            JsonWebKeyAlgorithm.RS512 => SecurityAlgorithms.RsaSha512,
            JsonWebKeyAlgorithm.ES256 => SecurityAlgorithms.EcdsaSha256,
            JsonWebKeyAlgorithm.ES384 => SecurityAlgorithms.EcdsaSha384,
            JsonWebKeyAlgorithm.ES512 => SecurityAlgorithms.EcdsaSha512,
            _ => ThrowHelper.ThrowArgumentException<string>(nameof(alg), "Unknown JWK Algorithm"),
        };

    public static string ToJwkString(this JsonWebKeyUse use)
        => use switch
        {
            JsonWebKeyUse.sig => JsonWebKeyUseNames.Sig,
            JsonWebKeyUse.enc => JsonWebKeyUseNames.Enc,
            _ => ThrowHelper.ThrowArgumentException<string>(nameof(use), "Unknown JWK Use"),
        };

    public static ECCurve ToECDsaCurve(this JsonWebKeyAlgorithm alg)
        => alg switch
        {
            JsonWebKeyAlgorithm.ES256 => ECCurve.NamedCurves.nistP256,
            JsonWebKeyAlgorithm.ES384 => ECCurve.NamedCurves.nistP384,
            JsonWebKeyAlgorithm.ES512 => ECCurve.NamedCurves.nistP521,
            _ => ThrowHelper.ThrowArgumentException<ECCurve>(nameof(alg), "Unknown JWK Algorithm"),
        };
}
