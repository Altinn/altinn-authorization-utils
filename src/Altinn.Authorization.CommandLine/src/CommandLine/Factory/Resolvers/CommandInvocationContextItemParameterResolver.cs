using System.CommandLine;
using Altinn.Authorization.CommandLine.Console;

namespace Altinn.Authorization.CommandLine.Factory.Resolvers;

/// <summary>
/// Defines a base class for parameter resolvers that resolve values from the <see cref="CommandInvocationContext"/>.
/// </summary>
internal abstract class CommandInvocationContextItemParameterResolver
    : ICommandHandlerParameterResolver
{
    private protected abstract void SetValue(CommandHandlerParameterResolverContext context);

    public Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
    {
        SetValue(context);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="CommandInvocationContext"/> parameter.
/// </summary>
internal sealed class CommandInvocationContextParameterResolver
    : CommandInvocationContextItemParameterResolver
{
    public static readonly CommandInvocationContextParameterResolver Instance = new();

    private protected override void SetValue(CommandHandlerParameterResolverContext context)
        => context.SetParameterValue(context.InvocationContext);
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="IConsole"/> parameter.
/// </summary>
internal sealed class ConsoleParameterResolver
    : CommandInvocationContextItemParameterResolver
{
    public static readonly ConsoleParameterResolver Instance = new();

    private protected override void SetValue(CommandHandlerParameterResolverContext context)
        => context.SetParameterValue(context.InvocationContext.Console);
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="IServiceProvider"/> parameter.
/// </summary>
internal sealed class ApplicationServicesParameterResolver
    : CommandInvocationContextItemParameterResolver
{
    public static readonly ApplicationServicesParameterResolver Instance = new();

    private protected override void SetValue(CommandHandlerParameterResolverContext context)
        => context.SetParameterValue(context.InvocationContext.ApplicationServices);
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="ParseResult"/> parameter.
/// </summary>
internal sealed class ParseResultParameterResolver
    : CommandInvocationContextItemParameterResolver
{
    public static readonly ParseResultParameterResolver Instance = new();

    private protected override void SetValue(CommandHandlerParameterResolverContext context)
        => context.SetParameterValue(context.InvocationContext.ParseResult);
}
