using Altinn.Cli.Jwks.Console;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ExportCommand
    : BaseCommand
{
    public ExportCommand(IConsole console, IServiceProvider services)
        : base(console, "export", "Export key sets")
    {
        foreach (var cmd in services.GetRequiredKeyedService<IEnumerable<Command>>(typeof(ExportCommand)))
        {
            Subcommands.Add(cmd);
        }

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

        Console.StdErr.WriteLine("Missing required subcommand.", new Style(foreground: Color.Red));
        Console.StdErr.WriteLine();

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
