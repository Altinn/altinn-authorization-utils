using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Cache settings, bound from <c>Altinn:Cache</c>.
/// </summary>
public sealed class AltinnCacheSettings
    : IValidatableObject
{
    /// <summary>
    /// The configuration section the settings are bound from.
    /// </summary>
    public const string SectionName = "Altinn:Cache";

    /// <summary>
    /// Gets or sets the connection settings for the distributed Valkey cache tier.
    /// </summary>
    [ValidateObjectMembers]
    public AltinnValkeyCacheSettings Valkey { get; set; } = new();

    /// <summary>
    /// Gets or sets the largest value (in bytes) written to the distributed tier. Larger values stay in the local
    /// tier only. Defaults to 1 MB.
    /// </summary>
    public int MaximumPayloadBytes { get; set; } = 1024 * 1024;

    /// <summary>
    /// Gets or sets how long a value lives in the distributed tier. Must be at least one minute. Defaults to five minutes.
    /// </summary>
    public TimeSpan DefaultDistributedCacheExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets how long a value lives in the local (in-memory) tier when there is no distributed tier.
    /// Defaults to five minutes. Must be at least ten seconds.
    /// </summary>
    public TimeSpan DefaultLocalCacheExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets how long a value lives in the local (in-memory) tier when the distributed tier is active. This
    /// bounds how long an instance can serve a value that another instance has removed or replaced. Defaults to
    /// 30 seconds. Must be at least ten seconds.
    /// </summary>
    public TimeSpan DefaultLocalCacheExpirationWithDistributedCache { get; set; } = TimeSpan.FromSeconds(30);

    /// <inheritdoc/>
    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
        if (DefaultDistributedCacheExpiration < TimeSpan.FromMinutes(1))
        {
            yield return new ValidationResult("Distributed cache expiration must be at least one minute", [nameof(DefaultDistributedCacheExpiration)]);
        }

        if (DefaultLocalCacheExpiration < TimeSpan.FromSeconds(10))
        {
            yield return new ValidationResult("Local cache expiration must be at least ten seconds", [nameof(DefaultLocalCacheExpiration)]);
        }

        if (DefaultLocalCacheExpirationWithDistributedCache < TimeSpan.FromSeconds(10))
        {
            yield return new ValidationResult("Local cache expiration with distributed cache must be at least ten seconds", [nameof(DefaultLocalCacheExpirationWithDistributedCache)]);
        }
    }
}
