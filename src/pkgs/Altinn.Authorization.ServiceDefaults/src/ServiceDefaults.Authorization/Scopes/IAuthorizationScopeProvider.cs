using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Defines a provider that supplies authorization scopes based on the specified authorization context.
/// </summary>
public interface IAuthorizationScopeProvider
{
    /// <summary>
    /// Retrieves the collection of authorization scopes associated with the specified authorization context.
    /// </summary>
    /// <param name="context">The authorization context containing information about the current request and user. Cannot be <see langword="null"/>.</param>
    /// <returns>An enumerable collection of strings representing the available space-separated scope-strings for the authorization context. The
    /// collection will be empty if no scopes are associated.</returns>
    public IEnumerable<string> GetScopeStrings(AuthorizationHandlerContext context);
}
