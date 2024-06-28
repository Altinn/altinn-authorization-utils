using Altinn.Cli.Jwks.Stores;
using Microsoft.IdentityModel.Tokens;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class CreateCommand
    : BaseCommand
{
    public static Argument<string> NameArg { get; }
        = new Argument<string>("name", "Name of the integration to generate JWKs for.");

    public static Option<bool> TestOption { get; }
        = new Option<bool>(
            aliases: ["--test", "-t", "--dev", "-d"],
            description: "Generate TEST keys. Defaults to true unless --prod is specified.");

    public static Option<bool> ProdOption { get; }
        = new Option<bool>(
            aliases: ["--prod", "-p"],
            description: "Generate PROD keys. Defaults to true unless --test is specified.");

    public static Option<int?> SizeOption { get; }
        = new Option<int?>(
            aliases: ["--size", "-s"],
            description: "Key size in bits.");

    public static Option<JsonWebKeyAlgorithm> AlgOption { get; }
        = new Option<JsonWebKeyAlgorithm>(
            aliases: ["--algorithm", "--alg", "-a"],
            description: "The algorithm to use for the key.",
            getDefaultValue: () => JsonWebKeyAlgorithm.RS256);

    public static Option<JsonWebKeyUse> UseOption { get; }
        = new Option<JsonWebKeyUse>(
            aliases: ["--use", "-u"],
            description: "Use for the JWK.",
            getDefaultValue: () => JsonWebKeyUse.sig);

    public CreateCommand()
        : base("create", "List all keys sets")
    {
        AddArgument(NameArg);
        AddOption(TestOption);
        AddOption(ProdOption);
        AddOption(SizeOption);
        AddOption(AlgOption);
        AddOption(UseOption);

        this.SetHandler(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(InvocationContext context)
    {
        var console = GetValueForHandlerParameter(Console, context);
        var store = GetValueForHandlerParameter(StoreOption, context);
        var name = GetValueForHandlerParameter(NameArg, context);
        var test = GetValueForHandlerParameter(TestOption, context);
        var prod = GetValueForHandlerParameter(ProdOption, context);
        var size = GetValueForHandlerParameter(SizeOption, context);
        var alg = GetValueForHandlerParameter(AlgOption, context);
        var use = GetValueForHandlerParameter(UseOption, context);
        var cancellationToken = GetValueForHandlerParameter(CancellationToken, context);

        return ExecuteAsync(
            console ?? throw new InvalidOperationException("console was null"),
            store ?? throw new InvalidOperationException("store was null"),
            name ?? throw new InvalidOperationException("name was null"),
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
        if (!test && !prod)
        {
            test = true;
            prod = true;
        }

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
