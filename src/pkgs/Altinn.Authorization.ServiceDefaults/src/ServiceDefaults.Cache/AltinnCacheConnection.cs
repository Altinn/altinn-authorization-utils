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
    /// Reads the password file and connects.
    /// </summary>
    public static async Task<IConnectionMultiplexer> ConnectAsync(ConfigurationOptions options, AltinnCacheSettings settings)
    {
        options.Password = ReadPassword(settings.PasswordFile!);

        return await ConnectionMultiplexer.ConnectAsync(options);
    }

    /// <summary>
    /// Reads the password from <paramref name="passwordFile"/>, ignoring surrounding whitespace and line breaks.
    /// </summary>
    public static string ReadPassword(string passwordFile)
        => File.ReadAllText(passwordFile).Trim();
}
