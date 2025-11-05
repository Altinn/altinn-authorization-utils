namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Specifies an authorization requirement that succeeds if the user possesses any one of the specified scopes.
/// </summary>
public sealed class AnyOfScopeAuthorizationAttribute
    : AuthorizationRequirementAttribute
    , IAnyOfScopeAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnyOfScopeAuthorizationAttribute"/> class.
    /// </summary>
    /// <param name="scopes">The list of scopes to check against.</param>
    public AnyOfScopeAuthorizationAttribute(params string[] scopes)
    {
        AnyOfScopes = ScopeSearchValues.Create(scopes);
    }

    /// <inheritdoc/>
    public ScopeSearchValues AnyOfScopes { get; }

    /// <inheritdoc/>
    public override string ToString()
        => $"{nameof(AnyOfScopeAuthorizationRequirement)}: Requires one of the following scopes: {string.Join(", ", AnyOfScopes)}";
}
