using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.JwkGenerator;

[ExcludeFromCodeCoverage]
static class Program
{
    private const string JWK_DIR_ENV_NAME = "ALTINN_JWK_DIR";

    internal static readonly Option<DirectoryInfo> JwkDirOption = new Option<DirectoryInfo>(
        aliases: ["--dir", "-D"],
        description: "Directory containing JWKs.",
        getDefaultValue: () =>
        {
            var fromEnv = Environment.GetEnvironmentVariable(JWK_DIR_ENV_NAME);
            if (!string.IsNullOrEmpty(fromEnv))
            {
                return new DirectoryInfo(fromEnv);
            }

            return new DirectoryInfo(Environment.CurrentDirectory);
        })
    {
        ArgumentHelpName = "DIR",
    };

    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Console app for creating Json Web Keys");
        rootCommand.AddGlobalOption(JwkDirOption);

        rootCommand.AddCommand(CreateJwkCommand.Command);
        rootCommand.AddCommand(KeyCommand.Command);
        rootCommand.AddCommand(ExportCommand.Command);

        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHelp(ctx =>
            {
                ctx.HelpBuilder.CustomizeSymbol(
                    JwkDirOption,
                    defaultValue: (_) => $"${JWK_DIR_ENV_NAME} || $PATH");
            })
            .Build();

        return await parser.InvokeAsync(args);
    }
}
