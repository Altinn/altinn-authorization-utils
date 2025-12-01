using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;
using Altinn.Swashbuckle.Security;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Swashbuckle.Security;

[ExcludeFromCodeCoverage]
internal sealed class SwaggerAnyOfScopeAuthorizationRequirementConditionProvider
    : OpenApiAuthorizationRequirementConditionProvider<IScopeAnyOfAuthorizationRequirement>
{
    private readonly IOptionsMonitor<AltinnSecurityOptions> _options;

    public SwaggerAnyOfScopeAuthorizationRequirementConditionProvider(IOptionsMonitor<AltinnSecurityOptions> options)
    {
        _options = options;
    }

    protected override IAsyncEnumerable<SecurityRequirementCondition> GetConditionsForAuthorizationRequirement(
        IScopeAnyOfAuthorizationRequirement requirement,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken = default)
    {
        var options = _options.Get(context.DocumentName);
        var defaultOptions = _options.CurrentValue;

        var enabled = options.EnableAltinnOidcScheme ?? defaultOptions.EnableAltinnOidcScheme ?? AltinnSecurityOptions.DefaultEnableAltinnOidcScheme;
        var schemeName = options.AltinnOidcSchemeName ?? defaultOptions.AltinnOidcSchemeName ?? AltinnSecurityOptions.DefaultAltinnOidcSchemeName;

        if (!enabled || string.IsNullOrEmpty(schemeName))
        {
            return AsyncEnumerable.Empty<SecurityRequirementCondition>();
        }

        return requirement.AnyOfScopes
            .Select(scope => SecurityRequirementCondition.Create(schemeName, scope))
            .ToAsyncEnumerable();
    }
}
