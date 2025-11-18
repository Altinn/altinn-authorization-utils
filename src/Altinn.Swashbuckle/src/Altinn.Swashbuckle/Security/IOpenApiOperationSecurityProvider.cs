using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Defines a provider for determining the security requirements of an OpenAPI operation based on its API description
/// and security context.
/// </summary>
public interface IOpenApiOperationSecurityProvider
{
    /// <summary>
    /// Gets security requirements for the specified API operation.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription"/> for which to get security requirements.</param>
    /// <param name="context">The context for this operation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="SecurityRequirement"/>s.</returns>
    public IAsyncEnumerable<SecurityRequirement> GetSecurityRequirementsForOperation(
        ApiDescription apiDescription,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken = default);
}
