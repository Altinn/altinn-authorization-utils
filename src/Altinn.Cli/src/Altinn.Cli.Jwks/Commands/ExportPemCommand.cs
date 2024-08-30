using Altinn.Cli.Jwks.Stores;
using Microsoft.IdentityModel.Tokens;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ExportPemCommand
    : BaseCommand
{
    public static Argument<string> NameArg { get; }
        = new Argument<string>("name", "Name of the integration to generate JWKs for.");

    public static Option<bool> ProdOption { get; }
        = new Option<bool>(
            aliases: ["--prod", "-p"],
            description: "Generate PROD keys. Defaults to true unless --test is specified.");

    public ExportPemCommand()
        : base("pem", "Export a public key in pem format")
    {
        AddArgument(NameArg);
        AddOption(ProdOption);

        this.SetHandler(ExecuteAsync, Console, StoreOption, NameArg, ProdOption, CancellationToken);
    }

    private async Task<int> ExecuteAsync(
        IConsole console,
        JsonWebKeySetStore store,
        string name,
        bool prod,
        CancellationToken cancellationToken)
    {
        var environment = prod ? JsonWebKeySetEnvironment.Prod : JsonWebKeySetEnvironment.Test;
        var keySet = await store.GetKeySet(name, environment, JsonWebKeySetVariant.Public, cancellationToken);

        var signingKey = keySet.GetSigningKeys()[^1];
        switch (signingKey)
        {
            case RsaSecurityKey rsa:
                WriteRsa(console, rsa);
                break;

            default:
                console.Error.WriteLine("Unsupported key type.");
                return 1;
        }

        return 0;
    }

    private void WriteRsa(IConsole console, RsaSecurityKey key)
    {
        var rsa = key.Rsa ?? RSA.Create(key.Parameters);
        var pem = rsa.ExportSubjectPublicKeyInfoPem();
        console.Out.WriteLine(pem);
    }
}
