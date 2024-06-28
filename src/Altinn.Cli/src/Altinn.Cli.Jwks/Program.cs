using Altinn.Cli.Jwks.Commands;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
static class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Console app for creating Json Web Keys");
        rootCommand.AddGlobalOption(BaseCommand.StoreOption);

        rootCommand.AddCommand(new CreateCommand());
        rootCommand.AddCommand(new ExportCommand());
        rootCommand.AddCommand(new ListCommand());

        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHost()
            .UseHelp(ctx =>
            {
                BaseCommand.StoreOption.UpdateHelp(ctx);
            })
            .Build();

        return await parser.InvokeAsync(args);
    }
}
