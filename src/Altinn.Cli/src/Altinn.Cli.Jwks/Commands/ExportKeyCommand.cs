using Altinn.Cli.Jwks.Stores;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ExportKeyCommand
    : BaseCommand
{
    public static Argument<string> NameArg { get; }
        = new Argument<string>("name", "Name of the integration to generate JWKs for.");

    public static Option<bool> ProdOption { get; }
        = new Option<bool>(
            aliases: ["--prod", "-p"],
            description: "Generate PROD keys. Defaults to true unless --test is specified.");

    public static Option<bool> Base64Option { get; }
        = new Option<bool>(
            aliases: ["--base64", "-b"],
            description: "Output the base64 version of the key.");

    public ExportKeyCommand()
        : base("key", "Export the current private key")
    {
        AddArgument(NameArg);
        AddOption(ProdOption);
        AddOption(Base64Option);

        this.SetHandler(ExecuteAsync, Console, StoreOption, NameArg, ProdOption, Base64Option, CancellationToken);
    }

    private async Task<int> ExecuteAsync(
        IConsole console,
        JsonWebKeySetStore store,
        string name,
        bool prod,
        bool base64,
        CancellationToken cancellationToken)
    {
        var environment = prod ? JsonWebKeySetEnvironment.Prod : JsonWebKeySetEnvironment.Test;
        await using var keyStream = await store.GetCurrentPrivateKeyReadStream(name, environment, cancellationToken);

        if (base64)
        {
            await OutBase64Stream(keyStream, cancellationToken);
        }
        else
        {
            await OutStream(keyStream, cancellationToken);
        }

        return 0;
    }

    private async Task OutBase64Stream(Stream stream, CancellationToken cancellationToken)
    {
        await using var outStream = System.Console.OpenStandardOutput();
        await using var writeStream = new CryptoStream(outStream, new ToBase64Transform(), CryptoStreamMode.Write);

        await stream.CopyToAsync(writeStream, cancellationToken);
    }

    private async Task OutStream(Stream stream, CancellationToken cancellationToken)
    {
        await using var outStream = System.Console.OpenStandardOutput();
        await stream.CopyToAsync(outStream, cancellationToken);
    }
}
