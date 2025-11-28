using Microsoft.AspNetCore.Authorization;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Provides a base class for supplying OpenAPI security requirement conditions for a specific type of authorization
/// requirement.
/// </summary>
/// <typeparam name="TRequirement">The type of authorization requirement for which security requirement conditions are provided.</typeparam>
public abstract class OpenApiAuthorizationRequirementConditionProvider<TRequirement>
    : IOpenApiAuthorizationRequirementConditionProvider
    where TRequirement : IAuthorizationRequirement
{
    /// <inheritdoc/>
    IAsyncEnumerable<SecurityRequirementCondition> IOpenApiAuthorizationRequirementConditionProvider.GetConditionsForAuthorizationRequirement(
        IAuthorizationRequirement requirement,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken)
    {
        if (requirement is TRequirement typedRequirement)
        {
            return GetConditionsForAuthorizationRequirement(typedRequirement, context, cancellationToken);
        }

        return AsyncEnumerable.Empty<SecurityRequirementCondition>();
    }

    /// <inheritdoc cref="IOpenApiAuthorizationRequirementConditionProvider.GetConditionsForAuthorizationRequirement(IAuthorizationRequirement, OpenApiSecurityContext, CancellationToken)"/>
    protected abstract IAsyncEnumerable<SecurityRequirementCondition> GetConditionsForAuthorizationRequirement(
        TRequirement requirement,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken = default);
}
