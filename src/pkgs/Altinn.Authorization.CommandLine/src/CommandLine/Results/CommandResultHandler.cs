using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Base class for handling command results of a specific type.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
public abstract class CommandResultHandler<T>
    : ICommandResultHandler
    , ICommandResultHandlerResolver
    where T : notnull
{
    Task ICommandResultHandler.HandleResult(object? result, CommandInvocationContext context, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(result);

        return HandleResult((T)result!, context, cancellationToken);
    }

    bool ICommandResultHandlerResolver.TryResolve(ICommandResultHandlerResolver resolvers, Type type, [NotNullWhen(true)] out ICommandResultHandler? handler)
    {
        if (type == typeof(T))
        {
            handler = this;
            return true;
        }

        handler = null;
        return false;
    }

    /// <summary>
    /// Handles the command result of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">The command result.</param>
    /// <param name="context">The command invocation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task HandleResult(T result, CommandInvocationContext context, CancellationToken cancellationToken = default);
}
