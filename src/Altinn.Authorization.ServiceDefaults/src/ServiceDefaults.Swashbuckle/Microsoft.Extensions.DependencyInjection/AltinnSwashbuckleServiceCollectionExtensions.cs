using Altinn.Authorization.ServiceDefaults.Swashbuckle.Security;
using Altinn.Authorization.ServiceDefaults.Swashbuckle.Servers;
using Altinn.Swashbuckle.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding example-data to the generated OpenAPI spec.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AltinnServiceDefaultsSwashbuckleServiceCollectionExtensions
{
    /// <summary>
    /// Adds Altinn server configuration to Swashbuckle-generated OpenAPI specifications.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration delegate.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddSwaggerAltinnServers(this IServiceCollection services, Action<AltinnServerOptions>? configureOptions = null)
    {
        var configure = services.AddOptions<AltinnServerOptions>();
        if (configureOptions != null)
        {
            configure.Configure(configureOptions);
        }

        services.AddSingleton<SwaggerAltinnServersDocumentFilter>();
        services.AddOptions<SwaggerGenOptions>()
            .Configure((SwaggerGenOptions options, IServiceProvider s) =>
            {
                options.AddDocumentFilterInstance(s.GetRequiredService<SwaggerAltinnServersDocumentFilter>());
            });

        return services;
    }

    /// <summary>
    /// Adds support for security definitions and requirements in swagger based on controller/endpoint authorization.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration delegate.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddSwaggeAltinnSecuritySupport(this IServiceCollection services, Action<AltinnSecurityOptions>? configureOptions = null)
    {
        var configure = services.AddOptions<AltinnSecurityOptions>();
        if (configureOptions != null)
        {
            configure.Configure(configureOptions);
        }

        services.AddOpenApiSecurityProvider();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOpenApiOperationSecurityProvider, SwaggerOpenApiRequirementProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOpenApiAuthorizationRequirementConditionProvider, SwaggerAnyOfScopeAuthorizationRequirementConditionProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOpenApiAuthorizationRequirementConditionProvider, SwaggerPlatformAccessTokenRequirementConditionProvider>());

        services.AddSingleton<SwaggerAltinnSecurityDocumentFilter>();
        services.AddSingleton<SwaggerAltinnOperationSecurityDescriptionFilter>();
        services.AddOptions<SwaggerGenOptions>()
            .Configure((SwaggerGenOptions options, IServiceProvider s) =>
            {
                options.AddDocumentFilterInstance(s.GetRequiredService<SwaggerAltinnSecurityDocumentFilter>());
                options.AddOperationAsyncFilterInstance(s.GetRequiredService<SwaggerAltinnOperationSecurityDescriptionFilter>());
            });

        return services;
    }
}
