using Altinn.Cli.Jwks.Console;
using Altinn.Cli.Jwks.Options;
using Altinn.Cli.Jwks.Stores;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class CreateCommand
    : BaseCommand
{
    public static Argument<string> NameArg { get; }
        = new("name")
        {
            Description = "Name of the integration to generate a new key for",
        };

    public static Option<JsonWebKeySetEnvironments> EnvOption { get; }
        = EnvOptions.Multiple;

    public static Option<int?> SizeOption { get; }
        = new(
            name: "--size",
            aliases: ["--size", "-s"])
        {
            Description = "Key size in bits",
            DefaultValueFactory = _ => null,
        };

    public static Option<JsonWebKeyAlgorithm> AlgOption { get; }
        = new(
            name: "--algorithm",
            aliases: ["--algorithm", "--alg", "-a"])
        {
            Description = "The algorithm to use for the key",
            DefaultValueFactory = _ => JsonWebKeyAlgorithm.RS256,
        };

    public static Option<JsonWebKeyUse> UseOption { get; }
        = new(
            name: "--use",
            aliases: ["--use", "-u"])
        {
            Description = "Use for the JWK",
            DefaultValueFactory = _ => JsonWebKeyUse.sig,
        };

    public static Option<string> SuffixOption { get; }
        = new(
            name: "--suffix",
            aliases: ["--suffix"])
        {
            Description = "Optional suffix to append to the key ID",
            DefaultValueFactory = _ => GetDateSuffix(),
        };

    public CreateCommand()
        : base("create", "Create a new key and add it to a keyset")
    {
        Arguments.Add(NameArg);
        Options.Add(EnvOption);
        Options.Add(SizeOption);
        Options.Add(AlgOption);
        Options.Add(UseOption);
        Options.Add(SuffixOption);

        SetAction(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(ParseResult result, CancellationToken cancellationToken)
    {
        var console = result.GetRequiredService<IConsole>();
        var store = result.GetRequiredValue(StoreOption);
        var name = result.GetRequiredValue(NameArg);
        var envs = result.GetRequiredValue(EnvOption);
        var size = result.GetRequiredValue(SizeOption);
        var alg = result.GetRequiredValue(AlgOption);
        var use = result.GetRequiredValue(UseOption);
        var suffix = result.GetRequiredValue(SuffixOption);

        return ExecuteAsync(
            console,
            store,
            name,
            envs,
            size,
            alg,
            use,
            suffix,
            cancellationToken);
    }

    private async Task<int> ExecuteAsync(
        IConsole console,
        JsonWebKeySetStore store,
        string name,
        JsonWebKeySetEnvironments envs,
        int? size,
        JsonWebKeyAlgorithm algorithm,
        JsonWebKeyUse use,
        string keySuffix,
        CancellationToken cancellationToken)
    {
        if (envs.HasFlag(JsonWebKeySetEnvironments.Test))
        {
            await UpdateKeySet(keySuffix, JsonWebKeySetEnvironment.Test);
        }

        if (envs.HasFlag(JsonWebKeySetEnvironments.Prod))
        {
            await UpdateKeySet(keySuffix, JsonWebKeySetEnvironment.Prod);
        }

        return 0;

        async Task UpdateKeySet(string suffix, JsonWebKeySetEnvironment environment)
        {
            var keyId = store.KeyId(name, environment, suffix);

            console.WriteLine($"Generating key {keyId}");
            var (privateKey, publicKey) = GenerateKeyPair(keyId);

            await store.AddKeyToKeySet(name, environment, privateKey, publicKey, cancellationToken);
        }

        (JsonWebKey privateKey, JsonWebKey publicKey) GenerateKeyPair(string keyId)
            => algorithm switch
            {
                JsonWebKeyAlgorithm.RS256 or JsonWebKeyAlgorithm.RS384 or JsonWebKeyAlgorithm.RS512 => GenerateRsaKeyPair(keyId),
                JsonWebKeyAlgorithm.ES256 or JsonWebKeyAlgorithm.ES384 or JsonWebKeyAlgorithm.ES512 => GenerateEcKeyPair(keyId),
                _ => throw new NotImplementedException($"Algorithm {algorithm} not implemented.")
            };

        (JsonWebKey privateKey, JsonWebKey publicKey) GenerateRsaKeyPair(string keyId)
        {
            var rsa = RSA.Create(size ?? 2048);
            var privRsa = new RsaSecurityKey(rsa.ExportParameters(true)) { KeyId = keyId };
            var pubRsa = new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = keyId };

            var privJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(privRsa);
            var pubJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(pubRsa);

            privJwk.Alg = pubJwk.Alg = algorithm.ToJwkString();
            privJwk.Use = pubJwk.Use = use.ToJwkString();

            return (privJwk, pubJwk);
        }

        (JsonWebKey privateKey, JsonWebKey publicKey) GenerateEcKeyPair(string keyId)
        {
            var pair = ECDsa.Create(algorithm.ToECDsaCurve());
            var privKey = ECDsa.Create(pair.ExportParameters(true));
            var pubKey = ECDsa.Create(pair.ExportParameters(false));

            var privEc = new ECDsaSecurityKey(privKey) { KeyId = keyId };
            var pubEc = new ECDsaSecurityKey(pubKey) { KeyId = keyId };

            var privJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(privEc);
            var pubJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(pubEc);

            privJwk.Alg = pubJwk.Alg = algorithm.ToJwkString();
            privJwk.Use = pubJwk.Use = use.ToJwkString();

            return (privJwk, pubJwk);
        }
    }

    private static string GetDateSuffix()
    {
        // get the number of minutes since 2025-01-01 (this fits in a 3-byte unsigned integer, and will do til past 2050)
        var totalMinutes = (uint)(DateTime.Now - new DateTime(2025, 1, 1)).TotalMinutes;
        var bytes = BitConverter.GetBytes(totalMinutes);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return Convert.ToBase64String(bytes.AsSpan(1, 3))
            .Replace("+", "-")
            .Replace("/", "_");
    }
}
