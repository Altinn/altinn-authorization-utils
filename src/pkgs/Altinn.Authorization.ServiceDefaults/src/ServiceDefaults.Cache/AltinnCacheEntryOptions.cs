using Microsoft.Extensions.Caching.Hybrid;

namespace Altinn.Authorization.ServiceDefaults.Cache;

/// <summary>
/// Creates <see cref="HybridCacheEntryOptions"/> that fit the registered cache tiers. Resolve it from the service
/// provider and take options from it instead of constructing <see cref="HybridCacheEntryOptions"/>: a per-call
/// <see cref="HybridCacheEntryOptions.Expiration"/> alone also sets the local (in-memory) lifetime, which would let
/// an instance serve a value another instance has removed for the full lifetime.
/// </summary>
public sealed class AltinnCacheEntryOptions
{
    private readonly TimeSpan? _localCacheExpiration;

    /// <param name="localCacheExpiration">
    /// The longest a value lives in the local tier when the distributed tier is active, or <see langword="null"/>
    /// when the cache is local only.
    /// </param>
    internal AltinnCacheEntryOptions(TimeSpan? localCacheExpiration)
    {
        _localCacheExpiration = localCacheExpiration;
    }

    /// <summary>
    /// Options for a value that lives <paramref name="expiration"/> in the cache. With the distributed tier active,
    /// the local copy lives at most the local lifetime (30 seconds by default), so a removal on another instance is
    /// seen within that time.
    /// </summary>
    /// <param name="expiration">How long the value lives.</param>
    public HybridCacheEntryOptions For(TimeSpan expiration)
        => _localCacheExpiration is { } local && local < expiration
            ? new() { Expiration = expiration, LocalCacheExpiration = local }
            : new() { Expiration = expiration };

    /// <summary>
    /// Options for a value kept in this instance's memory only, for <paramref name="expiration"/>. It is never written
    /// to or read from the distributed tier, and a removal on another instance is not seen.
    /// </summary>
    /// <param name="expiration">How long the value lives.</param>
    public HybridCacheEntryOptions LocalOnly(TimeSpan expiration)
        => new()
        {
            Expiration = expiration,
            LocalCacheExpiration = expiration,
            Flags = HybridCacheEntryFlags.DisableDistributedCache,
        };
}
