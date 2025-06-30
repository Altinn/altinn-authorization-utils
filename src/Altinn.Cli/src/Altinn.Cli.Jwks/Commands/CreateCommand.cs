using Altinn.Cli.Jwks.Console;
using Altinn.Cli.Jwks.Stores;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class CreateCommand
    : BaseCommand
{
    public static Argument<string> NameArg { get; }
        = new Argument<string>("name")
        {
            Description = "Name of the integration to generate JWKs for.",
        };

    public static Option<bool> TestOption { get; }
        = new Option<bool>(
            name: "--test",
            aliases: ["--test", "-t", "--dev", "-d"])
        {
            Description = "Generate TEST keys. Defaults to true unless --prod is specified.",
            DefaultValueFactory = r =>
            {
                var prodOptionResult = r.GetResult(ProdOption!);
                
                return prodOptionResult is null or { Implicit: true };
            },
        };

    public static Option<bool> ProdOption { get; }
        = new Option<bool>(
            name: "--prod",
            aliases: ["--prod", "-p"])
        {
            Description = "Generate PROD keys. Defaults to true unless --test is specified.",
            DefaultValueFactory = r => 
            {
                var testOptionResult = r.GetResult(TestOption!);
                
                return testOptionResult is null or { Implicit: true };
            },
        };

    public static Option<int?> SizeOption { get; }
        = new Option<int?>(
            name: "--size",
            aliases: ["--size", "-s"])
        {
            Description = "Key size in bits.",
            DefaultValueFactory = _ => null,
        };

    public static Option<JsonWebKeyAlgorithm> AlgOption { get; }
        = new Option<JsonWebKeyAlgorithm>(
            name: "--algorithm",
            aliases: ["--algorithm", "--alg", "-a"])
        {
            Description = "The algorithm to use for the key.",
            DefaultValueFactory = _ => JsonWebKeyAlgorithm.RS256,
        };

    public static Option<JsonWebKeyUse> UseOption { get; }
        = new Option<JsonWebKeyUse>(
            name: "--use",
            aliases: ["--use", "-u"])
        {
            Description = "Use for the JWK.",
            DefaultValueFactory = _ => JsonWebKeyUse.sig,
        };

    public CreateCommand()
        : base("create", "List all keys sets")
    {
        Arguments.Add(NameArg);
        Options.Add(TestOption);
        Options.Add(ProdOption);
        Options.Add(SizeOption);
        Options.Add(AlgOption);
        Options.Add(UseOption);

        SetAction(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(ParseResult result, CancellationToken cancellationToken)
    {
        var console = result.GetRequiredService<IConsole>();
        var store = result.GetRequiredValue(StoreOption);
        var name = result.GetRequiredValue(NameArg);
        var test = result.GetRequiredValue(TestOption);
        var prod = result.GetRequiredValue(ProdOption);
        var size = result.GetRequiredValue(SizeOption);
        var alg = result.GetRequiredValue(AlgOption);
        var use = result.GetRequiredValue(UseOption);

        return ExecuteAsync(
            console,
            store,
            name,
            test,
            prod,
            size,
            alg,
            use,
            cancellationToken);
    }

    private async Task<int> ExecuteAsync(
        IConsole console,
        JsonWebKeySetStore store,
        string name,
        bool test,
        bool prod,
        int? size,
        JsonWebKeyAlgorithm algorithm,
        JsonWebKeyUse use,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var keySuffix = string.Create(CultureInfo.InvariantCulture, $"{today:o}");

        if (test)
        {
            await UpdateKeySet(keySuffix, JsonWebKeySetEnvironment.Test);
        }

        if (prod)
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
}
