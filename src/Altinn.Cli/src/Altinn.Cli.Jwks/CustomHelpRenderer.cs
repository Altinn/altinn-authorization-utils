using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using Altinn.Cli.Jwks.Commands;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal class CustomHelpRenderer(HelpAction action) : SynchronousCommandLineAction
{
    private static readonly HelpOption DefaultHelpOption = new();
    private static readonly List<string> DefaultHelpCommands = [
        DefaultHelpOption.Name,
        .. DefaultHelpOption.Aliases
    ];

    public override int Invoke(ParseResult parseResult)
    {
        var isRootLevelHelp =
            parseResult.Tokens.Count == 1
            && DefaultHelpCommands.Contains(parseResult.Tokens[0].Value, StringComparer.OrdinalIgnoreCase);

        if (isRootLevelHelp)
        {
            BaseCommand.StoreOption.Hidden = true;
            int result = action.Invoke(parseResult);
            BaseCommand.StoreOption.Hidden = false;

            System.Console.WriteLine("Sample usage:");
            System.Console.WriteLine("  altinn-jwks create my-app-key");

            return result;
        }

        return action.Invoke(parseResult);
    }
}
