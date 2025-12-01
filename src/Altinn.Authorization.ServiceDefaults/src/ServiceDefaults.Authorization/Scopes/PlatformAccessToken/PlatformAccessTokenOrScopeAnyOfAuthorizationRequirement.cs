namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// An authorization requirement that allows access if the request includes a valid platform access token or
/// any one of the specified scopes.
/// </summary>
internal sealed class PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement
    : IScopeAnyOfAuthorizationRequirement
    , IPlatformAccessTokenRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement"/> class.
    /// </summary>
    /// <param name="scopes">The list of scopes to check against.</param>
    public PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement(scoped ReadOnlySpan<string> scopes)
    {
        AnyOfScopes = ScopeSearchValues.Create(scopes);
        ApprovedIssuers = ApprovedIssuersCheck.AllowAll;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeAnyOfAuthorizationRequirement"/> class.
    /// </summary>
    /// <param name="approvedIssuers">The approved issuers for the platform access token.</param>
    /// <param name="scopes">The list of scopes to check against.</param>
    public PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement(WellKnownPlatformAccessTokenIssuers approvedIssuers, scoped ReadOnlySpan<string> scopes)
    {
        AnyOfScopes = ScopeSearchValues.Create(scopes);
        ApprovedIssuers = ApprovedIssuersCheck.Create(approvedIssuers);
    }

    /// <inheritdoc/>
    public ScopeSearchValues AnyOfScopes { get; }

    /// <inheritdoc/>
    public ApprovedIssuersCheck ApprovedIssuers { get; }

    /// <inheritdoc/>
    public override string ToString()
        => $"{nameof(PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement)}: Requires platform-access-token, or any of scopes: {string.Join(", ", AnyOfScopes)}";
}
