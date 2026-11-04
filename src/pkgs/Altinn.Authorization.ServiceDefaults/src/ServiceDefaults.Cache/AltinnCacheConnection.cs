using StackExchange.Redis;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Builds the Valkey connection for the distributed cache tier.
/// </summary>
internal static class AltinnCacheConnection
{
    private const int TimeoutMilliseconds = 200;

    /// <summary>
    /// Creates the connection options: no TLS (the mesh encrypts), 200 ms connect, sync and async timeouts, no abort
    /// when the first connect fails, and fail fast while disconnected so a miss costs nothing beyond the factory.
    /// The password is filled in on connect by <see cref="ConnectAsync"/>.
    /// </summary>
    public static ConfigurationOptions CreateOptions(AltinnCacheSettings settings)
    {
        var options = new ConfigurationOptions
        {
            User = settings.Username,
            AbortOnConnectFail = false,
            ConnectTimeout = TimeoutMilliseconds,
            SyncTimeout = TimeoutMilliseconds,
            AsyncTimeout = TimeoutMilliseconds,
            Ssl = false,
            BacklogPolicy = BacklogPolicy.FailFast,
        };
        options.EndPoints.Add(settings.Host!);

        return options;
    }

    /// <summary>
    /// Fills in the password and connects.
    /// </summary>
    public static async Task<IConnectionMultiplexer> ConnectAsync(ConfigurationOptions options, AltinnCacheSettings settings)
    {
        options.Password = GetPassword(settings);

        return await ConnectionMultiplexer.ConnectAsync(options);
    }

    /// <summary>
    /// Returns <see cref="AltinnCacheSettings.Password"/> when set, otherwise reads
    /// <see cref="AltinnCacheSettings.PasswordFile"/>.
    /// </summary>
    public static string GetPassword(AltinnCacheSettings settings)
        => string.IsNullOrWhiteSpace(settings.Password) ? ReadPassword(settings.PasswordFile!) : settings.Password;

    /// <summary>
    /// Reads the password from <paramref name="passwordFile"/>, ignoring surrounding whitespace and line breaks.
    /// </summary>
    public static string ReadPassword(string passwordFile)
        => File.ReadAllText(passwordFile).Trim();
}
