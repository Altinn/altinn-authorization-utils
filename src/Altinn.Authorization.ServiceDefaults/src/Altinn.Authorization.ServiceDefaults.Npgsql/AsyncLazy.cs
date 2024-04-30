using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Npgsql;

internal class AsyncLazy<T>
    : IAsyncDisposable
{
    private readonly object _lock = new();
    private Func<CancellationToken, Task<T>>? _factory;
    private Task<T>? _task;

    public AsyncLazy(Func<CancellationToken, Task<T>> factory)
    {
        Guard.IsNotNull(factory);

        _factory = factory;
    }

    /// <summary>
    /// Gets already completed task or invokes the factory.
    /// </summary>
    /// <remark>
    /// The canceled task will be restarted automatically even if the lazy container is not resettable.
    /// </remark>
    /// <param name="token">The token that can be used to cancel the operation.</param>
    /// <returns>Lazy representation of the value.</returns>
    public Task<T> WithCancellation(CancellationToken cancellationToken)
        => Volatile.Read(ref _task) is { IsCanceled: false } t ? t.WaitAsync(cancellationToken) : GetOrStartAsync(cancellationToken);

    private Task<T> GetOrStartAsync(CancellationToken cancellationToken)
    {
        Task<T>? t;
        Func<CancellationToken, Task<T>>? factory;
        bool existingTask;

        lock (_lock)
        {
            // read barrier is provided by monitor
            t = _task; 
            factory = _factory;

            if (factory is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(AsyncLazy<T>));
            }

            if (t is { IsCanceled: false })
            {
                existingTask = true;
            }
            else
            {
                _task = t = Task.Run(CreateAsyncFunc(factory, cancellationToken));
                existingTask = false;
            }
        }

        // post-processing of task out of the lock
        if (existingTask)
        {
            t = t.WaitAsync(cancellationToken);
        }

        return t;

        // avoid capture of 'this' reference
        static Func<Task<T>> CreateAsyncFunc(Func<CancellationToken, Task<T>> cancelableFactory, CancellationToken cancellationToken)
            => () => cancelableFactory(cancellationToken);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        Task<T>? t;
        Func<CancellationToken, Task<T>>? factory;

        lock (_lock)
        {
            // read barrier is provided by monitor
            t = _task;
            factory = _factory;

            _task = null;
            _factory = null;
        }

        if (t is { IsCanceled: false })
        {
            T value;
            try
            {
                value = await t;
            }
            catch
            {
                return;
            }

            if (value is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync();
            }
            else if (value is IDisposable syncDisposable)
            {
                syncDisposable.Dispose();
            }
        }
    }
}
