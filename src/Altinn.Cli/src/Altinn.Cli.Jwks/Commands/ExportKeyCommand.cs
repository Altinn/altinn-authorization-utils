using Altinn.Cli.Jwks.Console;
using Altinn.Cli.Jwks.Stores;
using Nerdbank.Streams;
using Spectre.Console;
using System.Buffers;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ExportKeyCommand
    : BaseCommand
{
    public static Argument<string> NameArg { get; }
        = new Argument<string>("name")
        {
            Description = "Name of the integration to generate JWKs for.",
        };

    public static Option<bool> ProdOption { get; }
        = new Option<bool>(
            name: "--prod",
            aliases: ["--prod", "-p"])
        {
            Description = "Export PROD keys.",
            DefaultValueFactory = _ => false,
        };

    public static Option<bool> Base64Option { get; }
        = new Option<bool>(
            name: "--base64",
            aliases: ["--base64", "-b"])
        {
            Description = "Output the base64 version of the key.",
            DefaultValueFactory = _ => false,
        };

    public ExportKeyCommand()
        : base("key", "Export the current private key")
    {
        Arguments.Add(NameArg);
        Options.Add(ProdOption);
        Options.Add(Base64Option);

        SetAction(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(ParseResult result, CancellationToken cancellationToken)
    {
        var console = result.GetRequiredService<IConsole>();
        var store = result.GetRequiredValue(StoreOption);
        var name = result.GetRequiredValue(NameArg);
        var prod = result.GetRequiredValue(ProdOption);
        var base64 = result.GetRequiredValue(Base64Option);

        return ExecuteAsync(console, store, name, prod, base64, cancellationToken);
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
            console.StdErr.WriteLine($"Key-set {name} not found.");
            return 1;
        }

        if (base64)
        {
            await console.RunExclusive(() => OutBase64Stream(data.AsReadOnlySequence, cancellationToken));
        }
        else
        {
            await console.RunExclusive(() => OutStream(data.AsReadOnlySequence, cancellationToken));
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
