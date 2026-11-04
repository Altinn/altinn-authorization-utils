using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// The cache mode decided at registration, logged once at startup.
/// </summary>
/// <param name="Description">Human readable description of the active mode.</param>
internal sealed record AltinnCacheMode(string Description);

/// <summary>
/// Logs the active cache mode once at startup.
/// </summary>
internal sealed partial class AltinnCacheModeLogger(AltinnCacheMode mode, ILogger<AltinnCacheModeLogger> logger)
    : IHostedService
{
    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.CacheMode(logger, mode.Description);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Altinn cache: {Mode}")]
        public static partial void CacheMode(ILogger logger, string mode);
    }
}
