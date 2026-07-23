using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Resolves command result handlers from registered resolvers.
/// </summary>
public sealed class CommandResultHandler
{
    private readonly ImmutableArray<ICommandResultHandlerResolver> _resolvers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResultHandler"/> class.
    /// </summary>
    /// <param name="resolvers">The registered command result handler resolvers.</param>
    public CommandResultHandler(IEnumerable<ICommandResultHandlerResolver> resolvers)
    {
        _resolvers = [.. resolvers];
    }

    /// <summary>
    /// Attempts to resolve a command result handler for the specified result type.
    /// </summary>
    /// <param name="type">The result type.</param>
    /// <param name="handler">The resolved handler, when available.</param>
    /// <returns><see langword="true"/> if a handler was resolved; otherwise, <see langword="false"/>.</returns>
    public bool TryResolve(Type type, [NotNullWhen(true)] out ICommandResultHandler? handler)
    {
        foreach (var resolver in _resolvers)
        {
            if (resolver.TryResolve(type, out handler))
            {
                return true;
            }
        }

        handler = null!;
        return false;
    }
}

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

    bool ICommandResultHandlerResolver.TryResolve(Type type, [NotNullWhen(true)] out ICommandResultHandler? handler)
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
