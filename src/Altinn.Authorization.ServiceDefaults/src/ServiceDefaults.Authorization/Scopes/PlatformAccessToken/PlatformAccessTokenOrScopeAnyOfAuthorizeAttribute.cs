namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// Specifies an authorization requirement that allows access if the request includes a valid platform access token or
/// any one of the specified scopes.
/// </summary>
public sealed class PlatformAccessTokenOrScopeAnyOfAuthorizeAttribute
    : AuthorizationRequirementAttribute
    , IScopeAnyOfAuthorizationRequirement
    , IPlatformAccessTokenRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAccessTokenOrScopeAnyOfAuthorizeAttribute"/> class.
    /// </summary>
    /// <param name="scopes">The list of scopes to check against.</param>
    public PlatformAccessTokenOrScopeAnyOfAuthorizeAttribute(params string[] scopes)
    {
        AnyOfScopes = ScopeSearchValues.Create(scopes);
    }

    /// <inheritdoc/>
    public ScopeSearchValues AnyOfScopes { get; }

    /// <inheritdoc/>
    public ApprovedIssuersCheck ApprovedIssuers => ApprovedIssuersCheck.AllowAll;

    /// <inheritdoc/>
    public override string ToString()
        => $"{nameof(PlatformAccessTokenOrScopeAnyOfAuthorizeAttribute)}: Requires platform-access-token, or one of the following scopes: {string.Join(", ", AnyOfScopes)}";
}
