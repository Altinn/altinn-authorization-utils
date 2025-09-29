using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ServiceDefaults.AppConfiguration;

[ExcludeFromCodeCoverage]
internal sealed class RefreshAppConfigurationHostedService
    : IHostedService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);
    private static readonly TimerCallback _timerCallback = (object? state) =>
    {
        Debug.Assert(state is RefreshAppConfigurationHostedService);
        Unsafe.As<RefreshAppConfigurationHostedService>(state!).Refresh();
    };

    private readonly ImmutableArray<IConfigurationRefresher> _refreshers;
    private readonly TimeProvider _timeProvider;
    private readonly IHostApplicationLifetime _lifetime;

    private ITimer? _timer;

    public RefreshAppConfigurationHostedService(
        IConfigurationRefresherProvider refresherProvider,
        TimeProvider timeProvider,
        IHostApplicationLifetime lifetime)
    {
        _refreshers = [.. refresherProvider.Refreshers];
        _timeProvider = timeProvider;
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var timer = _timeProvider.CreateTimer(_timerCallback, state: this, RefreshInterval, RefreshInterval);
        Volatile.Write(ref _timer, timer);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return DisposeAsync().AsTask();
    }

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _timer, null) is { } timer)
        {
            return timer.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }

    private void Refresh()
    {
        var ct = _lifetime.ApplicationStopping;
        if (ct.IsCancellationRequested)
        {
            return;
        }

        //
        // Configuration refresh is meant to execute as an isolated background task.
        // To prevent access of request-based resources, such as HttpContext, we suppress the execution context within the refresh operation.
        using var flowControl = ExecutionContext.SuppressFlow();
        foreach (var refresher in _refreshers)
        {
            _ = Task.Run(() => refresher.TryRefreshAsync(ct), ct);
        }
    }
}
