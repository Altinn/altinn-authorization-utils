using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Re-reads the password file into the live <see cref="ConfigurationOptions"/> on an interval. The multiplexer reads
/// the password from those options on every handshake, so its next reconnect uses the rotated password.
/// </summary>
internal sealed partial class AltinnCachePasswordRefresher(
    ConfigurationOptions options,
    AltinnCacheSettings settings,
    ILogger<AltinnCachePasswordRefresher> logger)
    : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(settings.PasswordRefreshInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                options.Password = AltinnCacheConnection.ReadPassword(settings.PasswordFile!);
            }
            catch (IOException ex)
            {
                Log.PasswordFileReadFailed(logger, settings.PasswordFile!, ex);
            }
        }
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "Could not read cache password file '{PasswordFile}', keeping the previous password")]
        public static partial void PasswordFileReadFailed(ILogger logger, string passwordFile, Exception exception);
    }
}
