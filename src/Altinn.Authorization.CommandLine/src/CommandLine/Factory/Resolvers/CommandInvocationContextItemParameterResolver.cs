using System.CommandLine;
using Altinn.Authorization.CommandLine.Console;

namespace Altinn.Authorization.CommandLine.Factory.Resolvers;

/// <summary>
/// Defines a base class for parameter resolvers that resolve values from the <see cref="CommandInvocationContext"/>.
/// </summary>
internal abstract class CommandInvocationContextItemParameterResolver
    : ICommandHandlerParameterResolver
{
    protected abstract object? GetValue(CommandInvocationContext invocationContext);

    public Task<object?> ResolveParameterValue(CommandInvocationContext invocationContext, CancellationToken cancellationToken)
    {
        var value = GetValue(invocationContext);
        return Task.FromResult(value);
    }
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="CommandInvocationContext"/> parameter.
/// </summary>
internal sealed class CommandInvocationContextParameterResolver
    : CommandInvocationContextItemParameterResolver
{
    public static readonly CommandInvocationContextParameterResolver Instance = new();

    protected override object? GetValue(CommandInvocationContext invocationContext)
        => invocationContext;
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="IConsole"/> parameter.
/// </summary>
internal sealed class ConsoleParameterResolver
    : CommandInvocationContextItemParameterResolver
{
    public static readonly ConsoleParameterResolver Instance = new();

    protected override object? GetValue(CommandInvocationContext invocationContext)
        => invocationContext.Console;
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="IServiceProvider"/> parameter.
/// </summary>
internal sealed class ApplicationServicesParameterResolver
    : CommandInvocationContextItemParameterResolver
{
    public static readonly ApplicationServicesParameterResolver Instance = new();

    protected override object? GetValue(CommandInvocationContext invocationContext)
        => invocationContext.ApplicationServices;
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="ParseResult"/> parameter.
/// </summary>
internal sealed class ParseResultParameterResolver
    : CommandInvocationContextItemParameterResolver
{
    public static readonly ParseResultParameterResolver Instance = new();

    protected override object? GetValue(CommandInvocationContext invocationContext)
        => invocationContext.ParseResult;
}
