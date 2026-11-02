using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Diagnostics;
using StackExchange.Redis;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Connection settings for the distributed Valkey cache tier, bound from <c>Altinn:Cache:Valkey</c>.
/// </summary>
public sealed class AltinnValkeyCacheSettings
    : IValidatableObject
{
    private const int TimeoutMilliseconds = 200;

    /// <summary>
    /// Gets or sets whether the distributed Valkey cache tier is enabled. Defaults to false.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the version included in the cache key prefix. Bump it when the shape of a cached type changes.
    /// Must be at least one when Valkey is enabled. Defaults to one.
    /// </summary>
    public ushort KeyNamespaceVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the Valkey host, optionally with port (<c>host:6379</c>).
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the ACL user name.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Creates connection options with no TLS (the mesh encrypts), 200 ms timeouts, no abort when the first connect
    /// fails, and fail-fast behavior while disconnected.
    /// </summary>
    internal ConfigurationOptions CreateConfigurationOptions()
    {
        if (!Enabled)
        {
            ThrowHelper.ThrowInvalidOperationException("Valkey is not enabled");
        }

        var options = new ConfigurationOptions
        {
            User = Username,
            Password = Password,
            AbortOnConnectFail = false,
            ConnectTimeout = TimeoutMilliseconds,
            SyncTimeout = TimeoutMilliseconds,
            AsyncTimeout = TimeoutMilliseconds,
            Ssl = false,
            BacklogPolicy = BacklogPolicy.FailFast,
        };
        options.EndPoints.Add(Host!);

        return options;
    }

    /// <inheritdoc/>
    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
        if (!Enabled)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(Host))
        {
            yield return new ValidationResult("Host is required when Valkey is enabled", [nameof(Host)]);
        }

        if (KeyNamespaceVersion == 0)
        {
            yield return new ValidationResult("Key version must be at least one when Valkey is enabled", [nameof(KeyNamespaceVersion)]);
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            yield return new ValidationResult("Username is required when Valkey is enabled", [nameof(Username)]);
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult("Password is required when Valkey is enabled", [nameof(Password)]);
        }
    }
}
