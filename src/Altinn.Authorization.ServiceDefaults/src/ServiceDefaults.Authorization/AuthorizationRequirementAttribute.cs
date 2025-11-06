using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Authorization;

/// <summary>
/// Helper base class for authorization-requirement attributes.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class AuthorizationRequirementAttribute
    : Attribute
    , IAuthorizationRequirement
    , IAuthorizationRequirementData
{
    IEnumerable<IAuthorizationRequirement> IAuthorizationRequirementData.GetRequirements() => [this];
}
