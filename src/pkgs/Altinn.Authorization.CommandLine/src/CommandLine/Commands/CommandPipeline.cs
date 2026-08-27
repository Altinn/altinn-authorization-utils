using System.CommandLine;
using System.CommandLine.Invocation;
using Altinn.Authorization.CommandLine.Console;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine.Commands;

internal sealed class CommandPipeline
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CommandPipeline(IServiceScopeFactory scopeFactory)
    {
        Guard.IsNotNull(scopeFactory);

        _scopeFactory = scopeFactory;
    }

    internal CommandLineAction CreateAction(CommandHandlerDelegate handler)
        => new CommandHandlerAction(this, handler);

    internal async Task<int> Invoke(
        CommandHandlerDelegate handler,
        ParseResult parseResult,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = new CommandInvocationContext(parseResult, scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<IConsole>());
        await handler(context, cancellationToken);

        return context.ReturnCode;
    }

    private sealed class CommandHandlerAction(CommandPipeline commandPipeline, CommandHandlerDelegate handler)
        : AsynchronousCommandLineAction
    {
        public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            => commandPipeline.Invoke(handler, parseResult, cancellationToken);
    }
}
