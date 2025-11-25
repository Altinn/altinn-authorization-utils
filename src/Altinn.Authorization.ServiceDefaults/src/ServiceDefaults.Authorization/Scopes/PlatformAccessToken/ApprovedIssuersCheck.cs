using System.Buffers;
using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// Represents a policy for determining whether an access token issuer is approved.
/// </summary>
public abstract class ApprovedIssuersCheck
{
    /// <summary>
    /// Gets an instance of an issuer check that allows all issuers without restriction.
    /// </summary>
    public static ApprovedIssuersCheck AllowAll => AllowAllIssuers.Instance;

    /// <summary>
    /// Creates a new instance of the ApprovedIssuersCheck class using the specified set of well-known platform access
    /// token issuers.
    /// </summary>
    /// <param name="issuers">A combination of <see cref="WellKnownPlatformAccessTokenIssuers"/> flags that specifies which platform issuers to include.
    /// Must include at least one valid issuer.</param>
    /// <returns>An <see cref="ApprovedIssuersCheck"/> instance configured to validate tokens issued by the specified issuers.</returns>
    /// <exception cref="ArgumentException">Thrown if no issuers are specified in the issuers parameter.</exception>
    public static ApprovedIssuersCheck Create(WellKnownPlatformAccessTokenIssuers issuers)
    {
        var names = new List<string>();

        if (issuers.HasFlag(WellKnownPlatformAccessTokenIssuers.Platform))
        {
            names.Add("platform");
        }

        if (names.Count == 0)
        {
            throw new ArgumentException("At least one issuer must be specified.", nameof(issuers));
        }
        
        return Create(CollectionsMarshal.AsSpan(names));
    }

    /// <summary>
    /// Creates an instance of an issuer check that validates whether a given issuer is included in the specified set of
    /// approved issuers.
    /// </summary>
    /// <param name="approvedIssuers">A read-only span containing the set of approved issuer strings to match against. The comparison is
    /// case-sensitive and uses ordinal string comparison. If the span is empty, all issuers are considered approved.</param>
    /// <returns>An instance of an issuer check that allows only issuers present in the specified set. If no issuers are
    /// specified, an instance that allows all issuers is returned.</returns>
    public static ApprovedIssuersCheck Create(scoped ReadOnlySpan<string> approvedIssuers)
    {
        if (approvedIssuers.IsEmpty)
        {
            return AllowAll;
        }

#if NET9_0_OR_GREATER
        return new SearchValueApprovedIssuers(SearchValues.Create(approvedIssuers, StringComparison.Ordinal));
#else
        FrozenSet<string> frozenSet = approvedIssuers.ToArray().ToFrozenSet(StringComparer.Ordinal);
        return new FrozenSetApprovedIssuers(frozenSet);
#endif
    }

    /// <summary>
    /// Check if the provided issuer is approved.
    /// </summary>
    /// <param name="issuer">The issuer to check.</param>
    /// <returns><see langword="true"/> if <paramref name="issuer"/> is valid, otherwise <see langword="false"/>.</returns>
    public abstract bool Check(string issuer);

    private sealed class AllowAllIssuers
        : ApprovedIssuersCheck
    {
        public static readonly AllowAllIssuers Instance = new AllowAllIssuers();

        private AllowAllIssuers()
        {
        }

        /// <inheritdoc/>
        public override bool Check(string issuer) => true;
    }

#if NET9_0_OR_GREATER

    private sealed class SearchValueApprovedIssuers
        : ApprovedIssuersCheck
    {
        private readonly SearchValues<string> _approvedIssuers;

        public SearchValueApprovedIssuers(SearchValues<string> approvedIssuers)
        {
            _approvedIssuers = approvedIssuers;
        }

        /// <inheritdoc/>
        public override bool Check(string issuer)
            => _approvedIssuers.Contains(issuer);
    }

#else

    private sealed class FrozenSetApprovedIssuers
        : ApprovedIssuersCheck
    {
        private readonly FrozenSet<string> _approvedIssuers;

        public FrozenSetApprovedIssuers(FrozenSet<string> approvedIssuers)
        {
            _approvedIssuers = approvedIssuers;
        }

        /// <inheritdoc/>
        public override bool Check(string issuer)
            => _approvedIssuers.Contains(issuer);
    }

#endif
}
