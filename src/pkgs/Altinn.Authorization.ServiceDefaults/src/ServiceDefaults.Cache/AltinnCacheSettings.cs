namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Connection settings for the distributed (Valkey) cache tier, bound from <c>Altinn:Cache</c>.
/// </summary>
public sealed class AltinnCacheSettings
{
    /// <summary>
    /// The configuration section the settings are bound from.
    /// </summary>
    public const string SectionName = "Altinn:Cache";

    /// <summary>
    /// Gets or sets the Valkey host, optionally with port (<c>host:6379</c>).
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the ACL user name.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password, for local development. When set, <see cref="PasswordFile"/> is not used. In
    /// Kubernetes use <see cref="PasswordFile"/>, which is what makes rotation work without a restart.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the path of a file containing the password. The file is read on connect and then every
    /// <see cref="PasswordRefreshInterval"/>, so a rotated password is used by the next reconnect without a restart.
    /// </summary>
    public string? PasswordFile { get; set; }

    /// <summary>
    /// Gets or sets how often <see cref="PasswordFile"/> is re-read. Defaults to one minute.
    /// </summary>
    public TimeSpan PasswordRefreshInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets how long a value lives in the local (in-memory) tier when the distributed tier is active. This
    /// bounds how long an instance can serve a value that another instance has removed or replaced. Defaults to
    /// 30 seconds.
    /// </summary>
    public TimeSpan LocalCacheExpiration { get; set; } = TimeSpan.FromSeconds(30);
}
