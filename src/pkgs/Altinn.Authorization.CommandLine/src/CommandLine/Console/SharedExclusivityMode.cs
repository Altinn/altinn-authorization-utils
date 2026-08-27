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
        _semaphore.Wait();

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
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            return await func().ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release(1);
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
