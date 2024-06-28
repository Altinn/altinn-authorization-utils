using Altinn.Cli.Jwks.Stores;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ListCommand
    : BaseCommand
{
    public ListCommand()
        : base("list", "List all keys sets")
    {
        this.SetHandler(ExecuteAsync, Console, StoreOption, CancellationToken);
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
            console.Error.WriteLine("No keys found.");
        }

        return 0;
    }
}
