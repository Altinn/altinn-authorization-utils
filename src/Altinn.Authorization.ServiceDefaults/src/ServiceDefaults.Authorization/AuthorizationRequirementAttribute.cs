using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.ServiceDefaults.Authorization;

/// <summary>
/// Helper base class for authorization-requirement attributes.
/// </summary>
public abstract class AuthorizationRequirementAttribute
    : AuthorizeAttribute
    , IAuthorizationRequirement
    , IAuthorizationRequirementData
{
    IEnumerable<IAuthorizationRequirement> IAuthorizationRequirementData.GetRequirements() => [this];
}
