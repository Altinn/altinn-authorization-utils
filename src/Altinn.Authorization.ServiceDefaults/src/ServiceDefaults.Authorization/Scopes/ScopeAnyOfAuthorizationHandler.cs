using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Handles authorization of <see cref="IScopeAnyOfAuthorizationRequirement"/>.
/// </summary>
internal sealed class ScopeAnyOfAuthorizationHandler

    : AuthorizationHandler<IScopeAnyOfAuthorizationRequirement>
{
    private readonly IAuthorizationScopeProvider _scopeProvider;

    public ScopeAnyOfAuthorizationHandler(IAuthorizationScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IScopeAnyOfAuthorizationRequirement requirement)
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
