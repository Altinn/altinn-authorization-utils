using Microsoft.IdentityModel.Tokens;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;

namespace Altinn.Authorization.JwkGenerator;

[ExcludeFromCodeCoverage]
internal class CreateJwkCommand
{
    internal static readonly Option<DirectoryInfo> _outPathOption = new Option<DirectoryInfo>(
            aliases: ["--out", "-o"],
            description: "Output directory for the generated JWKs.",
            getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory));

    public static Command Command => CreateCommand();

    private static Command CreateCommand()
    {
        var nameArg = new Argument<string>("name", "Name of the integration to generate JWKs for.");

        var testOption = new Option<bool>(
            aliases: ["--test", "-t", "--dev", "-d"],
            description: "Generate TEST keys. Defaults to true unless --prod is specified.");

        var prodOption = new Option<bool>(
            aliases: ["--prod", "-p"],
            description: "Generate PROD keys. Defaults to true unless --test is specified.");

        var sizeOption = new Option<int?>(
            aliases: ["--size", "-s"],
            description: "Key size in bits.");

        var algOption = new Option<JWKAlgorithm>(
            aliases: ["--algorithm", "--alg", "-a"],
            description: "The algorithm to use for the key.",
            getDefaultValue: () => JWKAlgorithm.RS256);

        var useOption = new Option<JWKUse>(
            aliases: ["--use", "-u"],
            description: "Use for the JWK.",
            getDefaultValue: () => JWKUse.Sig);

        var cmd = new Command("create", "Creates a new JWK")
        {
            nameArg,
            testOption,
            prodOption,
            sizeOption,
            algOption,
            useOption,
            _outPathOption,
        };

        cmd.AddValidator(commandResult =>
        {
            var alg = commandResult.GetValueForOption(algOption);
            var size = commandResult.GetValueForOption(sizeOption);

            if (alg is JWKAlgorithm.RS256 or JWKAlgorithm.RS384 or JWKAlgorithm.RS512)
            {
                if (size is not (null or 2048 or 3072 or 4096))
                {
                    commandResult.ErrorMessage = "Key size must be 2048, 3072 or 4096 for RSA keys.";
                }
            }
            else if (alg is JWKAlgorithm.ES256 or JWKAlgorithm.ES384 or JWKAlgorithm.ES512)
            {
                if (size is not null)
                {
                    commandResult.ErrorMessage = "Key size must not be specified for EC keys.";
                }
            }
        });

        cmd.SetHandler(ExecuteAsync, nameArg, testOption, prodOption, sizeOption, algOption, useOption, _outPathOption);
        return cmd;
    }

    private static Task<int> ExecuteAsync(
        string name,
        bool test,
        bool prod,
        int? size,
        JWKAlgorithm algorithm,
        JWKUse use,
        DirectoryInfo outPath)
    {
        var command = new CreateJwkCommand(name, test, prod, size, algorithm, use, outPath);
        return command.ExecuteAsync();
    }

    private readonly string _name;
    private readonly bool _test;
    private readonly bool _prod;
    private readonly int? _size;
    private readonly JWKAlgorithm _algorithm;
    private readonly JWKUse _use;
    private readonly DirectoryInfo _outPath;

    public CreateJwkCommand(
        string name,
        bool test,
        bool prod,
        int? size,
        JWKAlgorithm algorithm,
        JWKUse use,
        DirectoryInfo outPath)
    {
        if (!test && !prod)
        {
            test = true;
            prod = true;
        }

        _name = name;
        _test = test;
        _prod = prod;
        _size = size;
        _algorithm = algorithm;
        _use = use;
        _outPath = outPath;
    }

    private async Task<int> ExecuteAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var keySuffix = FormattableString.Invariant($"{today:o}");

        if (_test)
        {
            await UpdateKeySet(keySuffix, "TEST");
        }

        if (_prod)
        {
            await UpdateKeySet(keySuffix, "PROD");
        }

        return 0;
    }

    private async Task UpdateKeySet(string suffix, string environment)
    {
        var keySetId = FormattableString.Invariant($"{_name}-{environment}");
        var keyId = FormattableString.Invariant($"{keySetId}.{suffix}");

        Console.WriteLine($"Generating key {keyId} for key-set {keySetId}");
        var (privateKey, publicKey) = GenerateKeyPair(keyId);

        var privateKeyPath = Path.Combine(_outPath.FullName, $"{keySetId}.json");
        var publicKeyPath = Path.Combine(_outPath.FullName, $"{keySetId}.pub.json");

        await AddOrUpdateKeySet(privateKeyPath, privateKey);
        await AddOrUpdateKeySet(publicKeyPath, publicKey);
    }

    private async Task AddOrUpdateKeySet(string keySetPath, JsonWebKey newKey)
    {
        await using var fs = File.Open(keySetPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        JsonWebKeySet keySet;
        if (fs.Length > 0)
        {
            // key-set exists, read and update
            keySet = await JsonSerializer.DeserializeAsync<JsonWebKeySet>(fs) ?? new();
        }
        else
        {
            keySet = new();
        }

        if (keySet.Keys.Any(k => k.Kid == newKey.Kid))
        {
            throw new InvalidOperationException($"Key with id {newKey.Kid} already exists in key-set.");
        }

        keySet.Keys.Add(newKey);

        // truncate the file
        fs.SetLength(0);
        await JsonSerializer.SerializeAsync(fs, keySet, Options);
    }

    private (JsonWebKey privateKey, JsonWebKey publicKey) GenerateKeyPair(string keyId)
        => _algorithm switch
        {
            JWKAlgorithm.RS256 or JWKAlgorithm.RS384 or JWKAlgorithm.RS512 => GenerateRsaKeyPair(keyId),
            JWKAlgorithm.ES256 or JWKAlgorithm.ES384 or JWKAlgorithm.ES512 => GenerateEcKeyPair(keyId),
            _ => throw new NotImplementedException($"Algorithm {_algorithm} not implemented.")
        };

    private (JsonWebKey privateKey, JsonWebKey publicKey) GenerateRsaKeyPair(string keyId)
    {
        var rsa = RSA.Create(_size ?? 2048);
        var privRsa = new RsaSecurityKey(rsa.ExportParameters(true)) { KeyId = keyId };
        var pubRsa = new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = keyId };

        var privJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(privRsa);
        var pubJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(pubRsa);

        privJwk.Alg = pubJwk.Alg = _algorithm.ToJwkString();
        privJwk.Use = pubJwk.Use = _use.ToJwkString();

        return (privJwk, pubJwk);
    }

    private (JsonWebKey privateKey, JsonWebKey publicKey) GenerateEcKeyPair(string keyId)
    {
        var pair = ECDsa.Create(_algorithm.ToECDsaCurve());
        var privKey = ECDsa.Create(pair.ExportParameters(true));
        var pubKey = ECDsa.Create(pair.ExportParameters(false));

        var privEc = new ECDsaSecurityKey(privKey) { KeyId = keyId };
        var pubEc = new ECDsaSecurityKey(pubKey) { KeyId = keyId };

        var privJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(privEc);
        var pubJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(pubEc);

        privJwk.Alg = pubJwk.Alg = _algorithm.ToJwkString();
        privJwk.Use = pubJwk.Use = _use.ToJwkString();

        return (privJwk, pubJwk);
    }

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
#if DEBUG
        WriteIndented = true,
#endif
    };
}
