using Microsoft.AspNetCore.Authorization;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Provides security requirements for an authorization policy by aggregating conditions from multiple OpenAPI
/// authorization requirement condition providers.
/// </summary>
internal class RequirementAuthorizationPolicySecurityProvider
    : IOpenApiAuthorizationPolicySecurityProvider
{
    private readonly ImmutableArray<IOpenApiAuthorizationRequirementConditionProvider> _providers;

    public RequirementAuthorizationPolicySecurityProvider(IEnumerable<IOpenApiAuthorizationRequirementConditionProvider> providers)
    {
        _providers = [.. providers];
    }

    public async IAsyncEnumerable<SecurityRequirement> GetSecurityRequirementsForAuthorizationPolicy(
        AuthorizationPolicy policy,
        OpenApiSecurityContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_providers.IsDefaultOrEmpty)
        {
            yield break;
        }

        List<SecurityRequirementCondition>? options = null;
        foreach (var requirement in policy.Requirements)
        {
            options?.Clear();

            foreach (var provider in _providers)
            {
                await foreach (var schemeRequirement in provider.GetConditionsForAuthorizationRequirement(requirement, context, cancellationToken))
                {
                    options ??= [];
                    options.Add(schemeRequirement);
                }
            }

            if (options is { Count: > 0 })
            {
                var display = requirement.ToString() ?? requirement.GetType().Name;
                yield return SecurityRequirement.Create(display, options);
            }
        }
    }
}
