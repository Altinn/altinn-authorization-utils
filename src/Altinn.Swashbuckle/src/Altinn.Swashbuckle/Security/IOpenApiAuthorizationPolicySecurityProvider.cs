using Microsoft.AspNetCore.Authorization;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Defines a provider that retrieves OpenAPI security requirements associated with a given authorization policy.
/// </summary>
public interface IOpenApiAuthorizationPolicySecurityProvider
{
    /// <summary>
    /// Gets security requirements for the specified authorization policy.
    /// </summary>
    /// <param name="policy">A <see cref="AuthorizationPolicy"/> for which to get security requirements.</param>
    /// <param name="context">The context for this operation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="SecurityRequirement"/>s.</returns>
    public IAsyncEnumerable<SecurityRequirement> GetSecurityRequirementsForAuthorizationPolicy(
        AuthorizationPolicy policy,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken = default);
}
