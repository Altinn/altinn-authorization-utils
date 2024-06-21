using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using static System.IdentityModel.Tokens.Jwt.JsonExtensions;

namespace Altinn.Authorization.JwkGenerator;

[ExcludeFromCodeCoverage]
static class Program
{
    static async Task Main(string[] args)
    {
        var keyNameOption = new Option<string>(
            name: "--keyName",
            description: "Name for the JWK. Used in the kid in the format: \"{keyName}-{date}\".",
            getDefaultValue: () => "MyJwk-DEV");

        var keySetNameOption = new Option<string>(
            name: "--keySetName",
            description: "Optional name for a keyset to include the key in. " +
            "Setting a keyset name will collate all keys with the same keyset name in a subfolder of the file output folder (see --filePath). " +
            "All public and private keys will also be added to separate collections stored in the base of the keyset folder. " +
            "Useful for when creating a new key for rotation, on an existing Maskinporten client.",
            getDefaultValue: () => "");

        var keySizeOption = new Option<int>(
            name: "--keySize",
            description: "Key size in bits.",
            getDefaultValue: () => 2048);

        var algOption = new Option<string>(
            name: "--alg",
            description: "Algorithm to use.",
            getDefaultValue: () => "RS256");

        var useOption = new Option<string>(
            name: "--use",
            description: "Use for the JWK.",
            getDefaultValue: () => "sig");

        var filePathOption = new Option<string>(
            name: "--filePath",
            description: "Filepath for where the JWK files should be stored.",
            getDefaultValue: () => @"c:\jwks");

        var createCmd = new Command("create", "Creates a new JWK.")
        {
            keyNameOption,
            keySizeOption,
            algOption,
            useOption,
            filePathOption,
            keySetNameOption
        };

        var rootCommand = new RootCommand("Console app for creating Json Web Keys");
        rootCommand.AddOption(keyNameOption);
        rootCommand.AddOption(keySizeOption);
        rootCommand.AddOption(algOption);
        rootCommand.AddOption(useOption);
        rootCommand.AddOption(filePathOption);
        rootCommand.AddOption(keySetNameOption);

        rootCommand.AddCommand(createCmd);

        createCmd.SetHandler(CreateJwk, keyNameOption, keySizeOption, algOption, useOption, filePathOption, keySetNameOption);

        await rootCommand.InvokeAsync(args);
    }
    static void CreateJwk(string keyName, int keySize, string alg, string use, string filePath, string keySetName)
    {
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        string kid = $"{keyName}-{date}";

        Console.WriteLine($"Creating JWK with");
        Console.WriteLine($"kid: {kid}, keySize: {keySize}, alg: {alg}, use: {use}, filePath: {filePath}");

        using RSA rsa = RSA.Create(keySize);

        WriteJwk(GetJwk(kid, rsa, alg, use, true), GetJwk(kid, rsa, alg, use, false), filePath, keySetName);
    }

    static JsonWebKey GetJwk(string kid, RSA rsa, string algorithm, string use, bool includePrivateKey)
    {
        RsaSecurityKey rsaSecKey = new(rsa.ExportParameters(includePrivateKey)) { KeyId = kid };
        JsonWebKey jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaSecKey);
        jwk.Alg = algorithm;
        jwk.Use = use;
        return jwk;
    }

    static void WriteJwk(JsonWebKey jwkPriv, JsonWebKey jwkPub, string basePath, string keySet)
    {
        string filePath = Path.Combine(basePath, jwkPriv.Kid);

        if (!string.IsNullOrWhiteSpace(keySet))
        {
            string keySetPath = Path.Combine(basePath, keySet);
            string privateKeySetPath = Path.Combine(keySetPath, "privateKeys.json");
            string publicKeySetPath = Path.Combine(keySetPath, "publicKeys.json");
            if (!Directory.Exists(keySetPath))
            {
                Console.WriteLine($"Creating new KeySet: {keySet}");
                Directory.CreateDirectory(keySetPath);

                File.WriteAllText(privateKeySetPath, SerializeToJson(new JsonWebKey[] { jwkPriv }));
                File.WriteAllText(publicKeySetPath, SerializeToJson(new JsonWebKey[] { jwkPub }));
            }
            else
            {
                Console.WriteLine($"Adding keys to existing KeySet: {keySet}");
                IList<JsonWebKey> privKeys = DeserializeFromJson<IList<JsonWebKey>>(File.ReadAllText(privateKeySetPath));
                IList<JsonWebKey> pubKeys = DeserializeFromJson<IList<JsonWebKey>>(File.ReadAllText(publicKeySetPath));
                privKeys.Add(jwkPriv);
                pubKeys.Add(jwkPub);
                File.WriteAllText(privateKeySetPath, SerializeToJson(privKeys));
                File.WriteAllText(publicKeySetPath, SerializeToJson(pubKeys));
            }

            filePath = Path.Combine(keySetPath, jwkPriv.Kid);
        }

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        string jwkPrivStr = SerializeToJson(jwkPriv);
        File.WriteAllText(Path.Combine(filePath, $"{jwkPriv.Kid}_priv.json"), jwkPrivStr);
        File.WriteAllText(Path.Combine(filePath, $"{jwkPriv.Kid}_priv_base64.txt"), Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jwkPrivStr)));
        File.WriteAllText(Path.Combine(filePath, $"{jwkPriv.Kid}_pub.json"), SerializeToJson(jwkPub));

        Console.WriteLine($"Success! Files written to {filePath}");
    }
}
