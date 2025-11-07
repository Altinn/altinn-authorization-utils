using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Swashbuckle.Security;

[ExcludeFromCodeCoverage]
internal sealed class SwaggerAltinnSecurityDocumentFilter
    : IDocumentFilter
{
    private readonly IOptionsMonitor<AltinnSecurityOptions> _options;

    public SwaggerAltinnSecurityDocumentFilter(IOptionsMonitor<AltinnSecurityOptions> options)
    {
        _options = options;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var options = _options.Get(context.DocumentName);
        var defaultOptions = _options.CurrentValue;

        if (options.EnableAltinnOidcScheme ?? defaultOptions.EnableAltinnOidcScheme ?? AltinnSecurityOptions.DefaultEnableAltinnOidcScheme)
        {
            var name = options.AltinnOidcSchemeName ?? defaultOptions.AltinnOidcSchemeName ?? AltinnSecurityOptions.DefaultAltinnOidcSchemeName;
            swaggerDoc.Components.SecuritySchemes.Add(name, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OpenIdConnect,
                OpenIdConnectUrl = options.AltinnOidcConfigurationUri ?? defaultOptions.AltinnOidcConfigurationUri ?? AltinnSecurityOptions.DefaultAltinnOidcConfigurationUri,
            });
        }

        if (options.EnablePlatformTokenScheme ?? defaultOptions.EnablePlatformTokenScheme ?? AltinnSecurityOptions.DefaultEnablePlatformTokenScheme)
        {
            var name = options.PlatformTokenSchemeName ?? defaultOptions.PlatformTokenSchemeName ?? AltinnSecurityOptions.DefaultPlatformTokenSchemeName;
            swaggerDoc.Components.SecuritySchemes.Add(name, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "PlatformAccessToken",
                In = ParameterLocation.Header,
            });
        }

        if (options.EnableAPIMScheme ?? defaultOptions.EnableAPIMScheme ?? AltinnSecurityOptions.DefaultEnableAPIMSCheme)
        {
            var name = options.APIMSchemeName ?? defaultOptions.APIMSchemeName ?? AltinnSecurityOptions.DefaultAPIMSchemeName;
            swaggerDoc.Components.SecuritySchemes.Add("apim", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "Ocp-Apim-Subscription-Key",
                In = ParameterLocation.Header,
            });
        }
    }
}
