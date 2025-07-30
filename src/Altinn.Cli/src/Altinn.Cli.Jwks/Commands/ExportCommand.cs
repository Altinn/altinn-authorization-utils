using Altinn.Cli.Jwks.Console;
using CommunityToolkit.Diagnostics;
using Spectre.Console;
using System;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ExportCommand
    : BaseCommand
{
    public ExportCommand()
        : base("export", "Export key sets")
    {
        Subcommands.Add(new ExportKeyCommand());

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult result, CancellationToken cancellationToken)
    {
        var helpOption = result.CommandResult.Command.Options.OfType<HelpOption>().FirstOrDefault()
            ?? result.RootCommandResult.Command.Options.OfType<HelpOption>().FirstOrDefault();

        if (helpOption is null)
        {
            ThrowHelper.ThrowInvalidOperationException("Missing subcommand");
        }

        var console = result.GetRequiredService<IConsole>();
        console.StdErr.WriteLine("Missing required subcommand.", new Style(foreground: Color.Red));
        console.StdErr.WriteLine();

        switch (helpOption.Action)
        {
            case SynchronousCommandLineAction syncAction:
                syncAction.Invoke(result);
                return 1;
            
            case AsynchronousCommandLineAction asyncAction:
                await asyncAction.InvokeAsync(result, cancellationToken);
                return 1;

            default:
                return ThrowHelper.ThrowInvalidOperationException<int>("Unknown command line action type");
        }
    }
}
