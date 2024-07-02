using Altinn.Cli.Jwks.Stores;
using Nerdbank.Streams;
using System.Buffers;
using System.CommandLine;
using System.CommandLine.IO;
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
        using var data = new Sequence<byte>(ArrayPool<byte>.Shared);
        if (!await store.GetCurrentPrivateKey(data, name, environment, cancellationToken))
        {
            console.Error.WriteLine($"Key-set {name} not found.");
            return 1;
        }

        if (base64)
        {
            await OutBase64Stream(data.AsReadOnlySequence, cancellationToken);
        }
        else
        {
            await OutStream(data.AsReadOnlySequence, cancellationToken);
        }

        return 0;
    }

    private Task OutBase64Stream(ReadOnlySequence<byte> data, CancellationToken cancellationToken)
    {
        using var base64 = new Sequence<byte>(ArrayPool<byte>.Shared);
        Base64Helper.EncodeUtf8(base64, data);
        return OutStream(base64.AsReadOnlySequence, cancellationToken);
    }

    private async Task OutStream(ReadOnlySequence<byte> data, CancellationToken cancellationToken)
    {
        await using var outStream = System.Console.OpenStandardOutput();
        await outStream.WriteAsync(data, cancellationToken);
    }
}
