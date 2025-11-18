using Altinn.Swashbuckle.Examples;
using Altinn.Swashbuckle.Filters;
using Altinn.Swashbuckle.Security;
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
    // TODO: https://github.com/dotnet/roslyn/issues/81217
    // When this is resolved, un-comment all the <returns> tags in the XML comments below.
    /// <param name="services">The service collection.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Add an <see cref="OpenApiExampleProvider"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <!--<returns><paramref name="services"/>.</returns>-->
        public OptionsBuilder<ExampleDataOptions> AddOpenApiExampleProvider()
        {
            var builder = services.AddExampleDataOptions();

            services.TryAddSingleton<OpenApiExampleProvider>();

            return builder;
        }

        /// <summary>
        /// Add support for attributes implementing <see cref="ISchemaFilter"/> to take effect during schema generation.
        /// </summary>
        /// <!--<returns><paramref name="services"/>.</returns>-->
        public IServiceCollection AddSwaggerFilterAttributeSupport()
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
        /// Adds documentation to swagger documents based on automatically discovered XML documentation.
        /// </summary>
        /// <!--<returns><paramref name="services"/>.</returns>-->
        public IServiceCollection AddSwaggerAutoXmlDoc()
        {
            services.AddXmlDocProvider();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<SwaggerGenOptions>, XmlDocFilterConfigurator>());

            return services;
        }

        /// <summary>
        /// Adds the default XML documentation provider to the service collection.
        /// </summary>
        /// <!--<returns><paramref name="services"/>.</returns>-->
        public IServiceCollection AddXmlDocProvider()
        {
            services.TryAddSingleton<IXmlDocProvider, DefaultXmlDocProvider>();

            return services;
        }

        /// <summary>
        /// Adds OpenAPI security providers for authorization policies and operations.
        /// </summary>
        /// <!--<returns><paramref name="services"/>.</returns>-->
        public IServiceCollection AddOpenApiSecurityProvider()
        {
            services.TryAddSingleton<OpenApiSecurityProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IOpenApiAuthorizationPolicySecurityProvider, RequirementAuthorizationPolicySecurityProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IOpenApiOperationSecurityProvider, AuthorizationOpenApiOperationSecurityProvider>());

            services.TryAddSingleton<SwaggerSecurityOperationFilter>();
            services.AddOptions<SwaggerGenOptions>()
                .Configure((SwaggerGenOptions options, IServiceProvider s) =>
                {
                    options.AddOperationAsyncFilterInstance(s.GetRequiredService<SwaggerSecurityOperationFilter>());
                });

            return services;
        }
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
