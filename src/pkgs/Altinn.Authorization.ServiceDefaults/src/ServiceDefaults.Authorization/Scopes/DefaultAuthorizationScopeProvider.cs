using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Default implementation of <see cref="IAuthorizationScopeProvider"/> that retrieves scopes from user claims.
/// </summary>
internal sealed class DefaultAuthorizationScopeProvider
    : IAuthorizationScopeProvider
{
    /// <inheritdoc/>
    public IEnumerable<string> GetScopeStrings(AuthorizationHandlerContext context)
    {
        foreach (var identity in context.User.Identities.Where(static i => string.Equals(i.AuthenticationType, "AuthenticationTypes.Federation")))
        {
            foreach (var claim in identity.Claims.Where(static c => string.Equals(c.Type, "urn:altinn:scope")))
            {
                yield return claim.Value;
            }
        }

        foreach (var claim in context.User.Claims.Where(static c => string.Equals(c.Type, "scope")))
        {
            yield return claim.Value;
        }
    }
}
