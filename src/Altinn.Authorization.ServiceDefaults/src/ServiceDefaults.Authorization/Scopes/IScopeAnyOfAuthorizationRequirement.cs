using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Defines a requirement for authorization that succeeds if the user possesses any of the specified scopes.
/// </summary>
/// <remarks>Use this interface to represent an authorization policy where access is granted if the user has at
/// least one of the required scopes. This is commonly used in resource-based authorization scenarios, such as OAuth or
/// OpenID Connect, where scopes represent permissions or access rights.</remarks>
public interface IScopeAnyOfAuthorizationRequirement
    : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the set of scopes that are used to match any required scope in an authorization check.
    /// </summary>
    public ScopeSearchValues AnyOfScopes { get; }
}
