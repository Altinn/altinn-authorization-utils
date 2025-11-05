namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Default implementation of <see cref="IAnyOfScopeAuthorizationRequirement"/>.
/// </summary>
internal sealed class AnyOfScopeAuthorizationRequirement
    : IAnyOfScopeAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnyOfScopeAuthorizationRequirement"/> class.
    /// </summary>
    /// <param name="scopes">The list of scopes to check against.</param>
    public AnyOfScopeAuthorizationRequirement(scoped ReadOnlySpan<string> scopes)
    {
        AnyOfScopes = ScopeSearchValues.Create(scopes);
    }

    /// <inheritdoc/>
    public ScopeSearchValues AnyOfScopes { get; }
}
