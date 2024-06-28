using Altinn.Cli.Jwks.Stores;
using System.Buffers.Text;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ExportMaskinportenCommand
    : BaseCommand
{
    public static Argument<string> NameArg { get; }
        = new Argument<string>("name", "Name of the integration to generate JWKs for.");

    public static Option<bool> ProdOption { get; }
        = new Option<bool>(
            aliases: ["--prod", "-p"],
            description: "Generate PROD keys. Defaults to true unless --test is specified.");

    public ExportMaskinportenCommand()
        : base("maskinporten", "Export a key set for maskinporten")
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

        await using var stdout = System.Console.OpenStandardOutput();
        await JsonSerializer.SerializeAsync(stdout, keySet.Keys, JsonOptions.Options, cancellationToken);
        return 0;
    }
}
