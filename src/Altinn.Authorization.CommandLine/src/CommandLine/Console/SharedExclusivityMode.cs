using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Console;

internal sealed class SharedExclusivityMode
    : IExclusivityMode
    , IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public T Run<T>(Func<T> func)
    {
        // Try acquiring the exclusivity semaphore
        if (!_semaphore.Wait(0))
        {
            ThrowExclusivityException();
        }

        try
        {
            return func();
        }
        finally
        {
            _semaphore.Release(1);
        }
    }

    public async Task<T> RunAsync<T>(Func<Task<T>> func)
    {
        // Try acquiring the exclusivity semaphore
        if (!await _semaphore.WaitAsync(0).ConfigureAwait(false))
        {
            ThrowExclusivityException();
        }

        try
        {
            return await func().ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release(1);
        }
    }

    [DoesNotReturn]
    private static void ThrowExclusivityException()
    {
        ThrowHelper.ThrowInvalidOperationException(
            "Trying to run one or more interactive functions concurrently. " +
            "Operations with dynamic displays (e.g. a prompt and a progress display) " +
            "cannot be running at the same time.");
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
