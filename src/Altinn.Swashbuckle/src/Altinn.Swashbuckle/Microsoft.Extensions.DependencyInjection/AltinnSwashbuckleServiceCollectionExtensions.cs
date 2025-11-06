using Altinn.Swashbuckle.Examples;
using Altinn.Swashbuckle.Filters;
using Altinn.Swashbuckle.Servers;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding example-data to the generated OpenAPI spec.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AltinnSwashbuckleServiceCollectionExtensions
{
    /// <summary>
    /// Add an <see cref="OpenApiExampleProvider"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static OptionsBuilder<ExampleDataOptions> AddOpenApiExampleProvider(this IServiceCollection services)
    {
        var builder = services.AddExampleDataOptions();

        services.TryAddSingleton<OpenApiExampleProvider>();

        return builder;
    }

    /// <summary>
    /// Add support for attributes implementing <see cref="ISchemaFilter"/> to take effect during schema generation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddSwaggerFilterAttributeSupport(this IServiceCollection services)
    {
        services.AddOpenApiExampleProvider();

        services.AddSingleton<SchemaFilterAttributeFilter>();
        services.AddSingleton<SwaggerStringAttributeFilter>();
        services.AddSingleton<SwaggerExampleFromExampleProviderFilter>();

        services.AddOptions<SwaggerGenOptions>()
            .Configure((SwaggerGenOptions options, IServiceProvider s) =>
            {
                options.SchemaGeneratorOptions.SchemaFilters.Add(s.GetRequiredService<SchemaFilterAttributeFilter>());
                options.SchemaGeneratorOptions.SchemaFilters.Add(s.GetRequiredService<SwaggerStringAttributeFilter>());
                options.SchemaGeneratorOptions.SchemaFilters.Add(s.GetRequiredService<SwaggerExampleFromExampleProviderFilter>());
            });

        return services;
    }

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
    /// Adds documentation to swagger documents based on automatically discovered XML documentation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddSwaggerAutoXmlDoc(this IServiceCollection services)
    {
        services.AddXmlDocProvider();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<SwaggerGenOptions>, XmlDocFilterConfigurator>());

        return services;
    }

    /// <summary>
    /// Adds the default XML documentation provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddXmlDocProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<IXmlDocProvider, DefaultXmlDocProvider>();

        return services;
    }

    private sealed class XmlDocFilterConfigurator
        : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IXmlDocProvider _xmlDocProvider;

        public XmlDocFilterConfigurator(IXmlDocProvider xmlDocProvider)
        {
            _xmlDocProvider = xmlDocProvider;
        }

        public void Configure(string? name, SwaggerGenOptions options)
        {
            options.AddParameterFilterInstance(new XmlDocParameterFilter(_xmlDocProvider));
            options.AddRequestBodyFilterInstance(new XmlDocRequestBodyFilter(_xmlDocProvider));
            options.AddOperationFilterInstance(new XmlDocOperationFilter(_xmlDocProvider));
            options.AddSchemaFilterInstance(new XmlDocSchemaFilter(_xmlDocProvider));
            options.AddDocumentFilterInstance(new XmlDocDocumentFilter(_xmlDocProvider, options.SwaggerGeneratorOptions));
        }

        public void Configure(SwaggerGenOptions options)
            => Configure(Options.Options.DefaultName, options);
    }
}
