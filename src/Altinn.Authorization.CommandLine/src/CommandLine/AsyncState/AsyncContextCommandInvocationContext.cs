using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.AsyncState;

namespace Altinn.Authorization.CommandLine.AsyncState;

internal sealed class AsyncContextCommandInvocationContext<T>
    : IAsyncContext<T>
    where T : class
{
    private readonly IAsyncLocalContext<T> _localContext;
    private readonly ICommandInvocationContextAccessor _commandInvocationContextAccessor;

    public AsyncContextCommandInvocationContext(
        IAsyncLocalContext<T> localContext,
        ICommandInvocationContextAccessor commandInvocationContextAccessor)
    {
        _localContext = localContext;
        _commandInvocationContextAccessor = commandInvocationContextAccessor;
    }

    /// <inheritdoc/>
    public T? Get()
    {
        if (!TryGet(out var value))
        {
            ThrowHelper.ThrowInvalidOperationException("Async context not available");
        }

        return value;
    }

    /// <inheritdoc/>
    public void Set(T? value)
    {
        var invocationContext = _commandInvocationContextAccessor.CommandInvocationContext;
        if (invocationContext == null)
        {
            // the call is made outside of an command invocation context
            _localContext.Set(value);
            return;
        }

        invocationContext.Extensions[typeof(TypeWrapper<T>)] = value;
    }

    /// <inheritdoc/>
    public bool TryGet(out T? value)
    {
        var invocationContext = _commandInvocationContextAccessor.CommandInvocationContext;
        if (invocationContext == null)
        {
            // the call is made outside of an command invocation context
            return _localContext.TryGet(out value);
        }

        value = (T?)invocationContext.Extensions[typeof(TypeWrapper<T>)];
        return true;
    }
}
