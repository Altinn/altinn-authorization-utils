using Altinn.Cli.Jwks.Console;
using Altinn.Cli.Jwks.Stores;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ExportMaskinportenCommand
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

    public ExportMaskinportenCommand()
        : base("maskinporten", "Export a key set for maskinporten")
    {
        Arguments.Add(NameArg);
        Options.Add(ProdOption);

        SetAction(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(ParseResult result, CancellationToken cancellationToken)
    {
        var console = result.GetRequiredService<IConsole>();
        var store = result.GetRequiredValue(StoreOption);
        var name = result.GetRequiredValue(NameArg);
        var prod = result.GetRequiredValue(ProdOption);

        return ExecuteAsync(console, store, name, prod, cancellationToken);
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

        await console.RunExclusive(async () =>
        {
            await using var stdout = System.Console.OpenStandardOutput();
            await JsonSerializer.SerializeAsync(stdout, keySet.Keys, JsonUtils.Options, cancellationToken);
        });
        
        return 0;
    }
}
