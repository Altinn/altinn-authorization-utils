namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Default implementation of <see cref="IScopeAnyOfAuthorizationRequirement"/>.
/// </summary>
internal sealed class ScopeAnyOfAuthorizationRequirement
    : IScopeAnyOfAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeAnyOfAuthorizationRequirement"/> class.
    /// </summary>
    /// <param name="scopes">The list of scopes to check against.</param>
    public ScopeAnyOfAuthorizationRequirement(scoped ReadOnlySpan<string> scopes)
    {
        AnyOfScopes = ScopeSearchValues.Create(scopes);
    }

    /// <inheritdoc/>
    public ScopeSearchValues AnyOfScopes { get; }
}
