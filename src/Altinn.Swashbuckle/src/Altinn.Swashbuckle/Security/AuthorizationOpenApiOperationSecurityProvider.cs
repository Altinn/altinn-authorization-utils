using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Provides OpenAPI operation security requirements based on ASP.NET Core authorization policies and endpoint metadata.
/// </summary>
[ExcludeFromCodeCoverage]
internal class AuthorizationOpenApiOperationSecurityProvider
    : IOpenApiOperationSecurityProvider
{
    private readonly IAuthorizationPolicyProvider _policyProvider;
    private readonly ImmutableArray<IOpenApiAuthorizationPolicySecurityProvider> _policySecurityProviders;

    public AuthorizationOpenApiOperationSecurityProvider(
        IAuthorizationPolicyProvider policyProvider,
        IEnumerable<IOpenApiAuthorizationPolicySecurityProvider> policySecurityProviders)
    {
        _policyProvider = policyProvider;
        _policySecurityProviders = [.. policySecurityProviders];
    }

    public async IAsyncEnumerable<SecurityRequirement> GetSecurityRequirementsForOperation(
        ApiDescription apiDescription,
        OpenApiSecurityContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var policy = await GetPolicy(apiDescription, cancellationToken);
        if (policy == null)
        {
            yield break;
        }

        foreach (var provider in _policySecurityProviders)
        {
            await foreach (var requirement in provider.GetSecurityRequirementsForAuthorizationPolicy(policy, context, cancellationToken))
            {
                yield return requirement;
            }
        }
    }

    private async Task<AuthorizationPolicy?> GetPolicy(ApiDescription apiDescription, CancellationToken cancellationToken)
    {
        var endpointMetadata = apiDescription.ActionDescriptor.EndpointMetadata;
        var authorizeData = endpointMetadata.OfType<IAuthorizeData>();
        var policies = endpointMetadata.OfType<AuthorizationPolicy>();

        var policy = await AuthorizationPolicy.CombineAsync(_policyProvider, authorizeData, policies);

        var requirementData = endpointMetadata.OfType<IAuthorizationRequirementData>();
        if (requirementData.Any())
        {
            var reqPolicy = new AuthorizationPolicyBuilder();
            foreach (var rd in requirementData)
            {
                foreach (var r in rd.GetRequirements())
                {
                    reqPolicy.AddRequirements(r);
                }
            }

            policy = policy is null
                ? reqPolicy.Build()
                : AuthorizationPolicy.Combine(policy, reqPolicy.Build());
        }

        return policy;
    }
}
