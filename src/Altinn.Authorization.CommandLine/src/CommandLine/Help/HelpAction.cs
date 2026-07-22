using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Altinn.Authorization.CommandLine.Console;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Represents a command line action that displays help text.
/// </summary>
public sealed class ExtendedHelpAction(IServiceScopeFactory scopeFactory)
    : AsynchronousCommandLineAction
{
    /// <inheritdoc />
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var console = services.GetRequiredService<IConsole>();
        var builder = services.GetRequiredService<IHelpBuilder>();

        var output = parseResult.Errors.Count > 0 ? console.StdErr : console.StdOut;
        var helpContext = new HelpContext(builder, parseResult.CommandResult.Command, output.Profile);

        var help = builder.Build(helpContext);
        output.Write(help);

        return 0;
    }

    /// <inheritdoc />
    public override bool ClearsParseErrors => true;

    internal void Configure(ICommandConventionBuilder builder)
    {
        builder.Finally(builder =>
        {
            foreach (var helpOption in builder.Options.OfType<HelpOption>())
            {
                helpOption.Action = this;
            }
        }, recursive: true);
    }
}
