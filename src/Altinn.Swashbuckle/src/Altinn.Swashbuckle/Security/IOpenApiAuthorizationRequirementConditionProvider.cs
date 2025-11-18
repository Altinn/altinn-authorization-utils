using Microsoft.AspNetCore.Authorization;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Defines a provider for determining security requirement-conditions associated with a specific authorization requirement.
/// </summary>
public interface IOpenApiAuthorizationRequirementConditionProvider
{
    /// <summary>
    /// Gets security requirement-conditions for the specified authorization requirement.
    /// </summary>
    /// <param name="requirement">A <see cref="IAuthorizationRequirement"/> for which to get <see cref="SecurityRequirementCondition"/>s.</param>
    /// <param name="context">The context for this operation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="SecurityRequirementCondition"/>s.</returns>
    public IAsyncEnumerable<SecurityRequirementCondition> GetConditionsForAuthorizationRequirement(
        IAuthorizationRequirement requirement,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken = default);
}
