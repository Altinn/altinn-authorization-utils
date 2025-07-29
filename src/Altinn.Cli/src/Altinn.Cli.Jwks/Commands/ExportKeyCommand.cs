using Altinn.Cli.Jwks.Console;
using Altinn.Cli.Jwks.Options;
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
        = new("name")
        {
            Description = "Name of the key to export",
        };

    public static Option<JsonWebKeySetEnvironment> EnvironmentOption { get; }
        = EnvOptions.Single;

    public static Option<JsonWebKeySetVariant> KeyVariantOption { get; }
        = new(
            name: "--variant",
            aliases: ["--variant", "-role"])
        {
            Description = "Decides whether to export the private or the public key",
            DefaultValueFactory = _ => JsonWebKeySetVariant.Private,
        };

    public static Option<bool> Base64Option { get; }
        = new(
            name: "--base64",
            aliases: ["--base64", "-b"])
        {
            Description = "Outputs the base64 version of the key",
            DefaultValueFactory = _ => false,
        };

    public ExportKeyCommand()
        : base("key", "Export the current private or public key")
    {
        Arguments.Add(NameArg);
        Options.Add(EnvironmentOption);
        Options.Add(KeyVariantOption);
        Options.Add(Base64Option);

        SetAction(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(ParseResult result, CancellationToken cancellationToken)
    {
        var console = result.GetRequiredService<IConsole>();
        var store = result.GetRequiredValue(StoreOption);
        var name = result.GetRequiredValue(NameArg);
        var env = result.GetRequiredValue(EnvironmentOption);
        var base64 = result.GetRequiredValue(Base64Option);

        return ExecuteAsync(console, store, name, env, base64, cancellationToken);
    }

    private async Task<int> ExecuteAsync(
        IConsole console,
        JsonWebKeySetStore store,
        string name,
        JsonWebKeySetEnvironment environment,
        bool base64,
        CancellationToken cancellationToken)
    {
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
