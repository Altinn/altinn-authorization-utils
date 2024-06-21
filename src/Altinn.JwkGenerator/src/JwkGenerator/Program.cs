using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.JwkGenerator;

[ExcludeFromCodeCoverage]
static class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Console app for creating Json Web Keys");

        rootCommand.AddCommand(CreateJwkCommand.Command);

        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHelp(ctx =>
            {
                ctx.HelpBuilder.CustomizeSymbol(
                    CreateJwkCommand._outPathOption,
                    defaultValue: (_) => "$PATH");
            })
            .Build();

        return await parser.InvokeAsync(args);
    }
}
