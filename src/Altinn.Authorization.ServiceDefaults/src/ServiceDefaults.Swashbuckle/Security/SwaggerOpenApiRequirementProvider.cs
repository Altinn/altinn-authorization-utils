using Altinn.Swashbuckle.Security;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Swashbuckle.Security;

[ExcludeFromCodeCoverage]
internal sealed class SwaggerOpenApiRequirementProvider
    : IOpenApiOperationSecurityProvider
{
    private readonly IOptionsMonitor<AltinnSecurityOptions> _options;

    public SwaggerOpenApiRequirementProvider(IOptionsMonitor<AltinnSecurityOptions> options)
    {
        _options = options;
    }

    public IAsyncEnumerable<SecurityRequirement> GetSecurityRequirementsForOperation(
        ApiDescription apiDescription,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken = default)
    {
        var options = _options.Get(context.DocumentName);
        var defaultOptions = _options.CurrentValue;

        var enabled = options.EnableAPIMScheme ?? defaultOptions.EnableAPIMScheme ?? AltinnSecurityOptions.DefaultEnableAPIMSCheme;
        var schemeName = options.APIMSchemeName ?? defaultOptions.APIMSchemeName ?? AltinnSecurityOptions.DefaultAPIMSchemeName;

        if (!enabled || string.IsNullOrEmpty(schemeName))
        {
            return AsyncEnumerable.Empty<SecurityRequirement>();
        }

        return AsyncEnumerable.ToAsyncEnumerable([
            SecurityRequirement.Create(
                "API Management Subscription Key",
                [SecurityRequirementCondition.Create(schemeName)]
            )
        ]);
    }
}
