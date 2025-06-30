using Altinn.Cli.Jwks.Console;
using Altinn.Cli.Jwks.Stores;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ListCommand
    : BaseCommand
{
    public ListCommand()
        : base("list", "List all keys sets")
    {
        SetAction(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(ParseResult result, CancellationToken cancellationToken)
    {
        var console = result.GetRequiredService<IConsole>();
        var store = result.GetRequiredValue(StoreOption);

        return ExecuteAsync(console, store, cancellationToken);
    }

    private async Task<int> ExecuteAsync(IConsole console, JsonWebKeySetStore store, CancellationToken cancellationToken)
    {
        var count = 0;
        await foreach (var (name, envs) in store.List(cancellationToken: cancellationToken))
        {
            var variantsString = envs switch
            {
                JsonWebKeySetEnvironments.Test => "TEST",
                JsonWebKeySetEnvironments.Prod => "PROD",
                JsonWebKeySetEnvironments.Test | JsonWebKeySetEnvironments.Prod => "TEST, PROD",
                _ => null,
            };

            if (variantsString is null)
            {
                continue;
            }

            console.WriteLine($"{name} ({variantsString})");
            count++;
        }

        if (count == 0)
        {
            console.StdErr.WriteLine("No keys found.");
        }

        return 0;
    }
}
