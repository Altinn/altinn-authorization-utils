using Altinn.Cli.Jwks.Commands;
using Altinn.Cli.Jwks.Console;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
static class Program
{
    static async Task<int> Main(string[] args)
    {
        using var cancellationSignalHandler = new CancellationSignalHandler();

        var rootCommand = new RootCommand("Console app for creating Json Web Keys")
            .AddOptionMiddleware<HelpOption>(x => new CustomHelpRenderer((HelpAction)x.Action!));

        rootCommand.TreatUnmatchedTokensAsErrors = true;

        rootCommand.Options.Add(BaseCommand.StoreOption);
        rootCommand.Subcommands.Add(new CreateCommand());
        rootCommand.Subcommands.Add(new ExportCommand());
        rootCommand.Subcommands.Add(new ListCommand());

        var config = new CommandLineConfiguration(rootCommand)
            .UseHost(builder =>
            {
                builder.ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IConsole, Console.Console>();
                    services.AddSingleton(s => s.GetRequiredService<IConsole>().StdOut);
                    services.AddKeyedSingleton(ConsoleTarget.StdOut, (s, _) => s.GetRequiredService<IConsole>().StdOut);
                    services.AddKeyedSingleton(ConsoleTarget.StdErr, (s, _) => s.GetRequiredService<IConsole>().StdErr);
                });
            });

        config.ThrowIfInvalid();

        return await config.InvokeAsync(args, cancellationSignalHandler.Token);
    }
}
