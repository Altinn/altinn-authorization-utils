using Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;
using Altinn.Swashbuckle.Security;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Swashbuckle.Security;

[ExcludeFromCodeCoverage]
internal sealed class SwaggerPlatformAccessTokenRequirementConditionProvider
    : OpenApiAuthorizationRequirementConditionProvider<IPlatformAccessTokenRequirement>
{
    private readonly IOptionsMonitor<AltinnSecurityOptions> _options;

    public SwaggerPlatformAccessTokenRequirementConditionProvider(IOptionsMonitor<AltinnSecurityOptions> options)
    {
        _options = options;
    }

    protected override IAsyncEnumerable<SecurityRequirementCondition> GetConditionsForAuthorizationRequirement(IPlatformAccessTokenRequirement requirement, OpenApiSecurityContext context, CancellationToken cancellationToken = default)
    {
        var options = _options.Get(context.DocumentName);
        var defaultOptions = _options.CurrentValue;

        var enabled = options.EnablePlatformTokenScheme ?? defaultOptions.EnablePlatformTokenScheme ?? AltinnSecurityOptions.DefaultEnablePlatformTokenScheme;
        var schemeName = options.PlatformTokenSchemeName ?? defaultOptions.PlatformTokenSchemeName ?? AltinnSecurityOptions.DefaultPlatformTokenSchemeName;

        if (!enabled || string.IsNullOrEmpty(schemeName))
        {
            return AsyncEnumerable.Empty<SecurityRequirementCondition>();
        }

        return AsyncEnumerable.ToAsyncEnumerable([SecurityRequirementCondition.Create(schemeName)]);
    }
}
