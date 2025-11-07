using Altinn.Authorization.ServiceDefaults.Swashbuckle.Security;
using Altinn.Authorization.ServiceDefaults.Swashbuckle.Servers;
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
    /// <param name="configureServers">Optional configuration delegate for the servers.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddSwaggerAltinnServers(this IServiceCollection services, Action<AltinnServerOptions>? configureServers = null)
    {
        var configure = services.AddOptions<AltinnServerOptions>();
        if (configureServers != null)
        {
            configure.Configure(configureServers);
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
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddSwaggeAltinnSecuritySupport(this IServiceCollection services)
    {
        services.AddSingleton<SwaggerAltinnSecurityDocumentFilter>();
        services.AddOptions<SwaggerGenOptions>()
            .Configure((SwaggerGenOptions options, IServiceProvider s) =>
            {
                options.AddDocumentFilterInstance(s.GetRequiredService<SwaggerAltinnSecurityDocumentFilter>());
            });

        return services;
    }
}
