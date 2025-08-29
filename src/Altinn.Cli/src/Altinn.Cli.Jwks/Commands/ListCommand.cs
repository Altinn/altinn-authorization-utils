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
    public ListCommand(IConsole console)
        : base(console, "list", "List all keys sets")
    {
        SetAction(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(ParseResult result, CancellationToken cancellationToken)
    {
        var store = result.GetRequiredValue(StoreOption);

        return ExecuteAsync(store, cancellationToken);
    }

    private async Task<int> ExecuteAsync(JsonWebKeySetStore store, CancellationToken cancellationToken)
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

            Console.WriteLine($"{name} ({variantsString})");
            count++;
        }

        if (count == 0)
        {
            Console.StdErr.WriteLine("No keys found.");
        }

        return 0;
    }
}
