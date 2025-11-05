using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Handles authorization of <see cref="IAnyOfScopeAuthorizationRequirement"/>.
/// </summary>
internal sealed class AnyOfScopeAuthorizationHandler

    : AuthorizationHandler<IAnyOfScopeAuthorizationRequirement>
{
    private readonly IAuthorizationScopeProvider _scopeProvider;

    public AnyOfScopeAuthorizationHandler(IAuthorizationScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IAnyOfScopeAuthorizationRequirement requirement)
    {
        foreach (string scopeString in _scopeProvider.GetScopeStrings(context))
        {
            if (requirement.AnyOfScopes.Check(scopeString))
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }
}
