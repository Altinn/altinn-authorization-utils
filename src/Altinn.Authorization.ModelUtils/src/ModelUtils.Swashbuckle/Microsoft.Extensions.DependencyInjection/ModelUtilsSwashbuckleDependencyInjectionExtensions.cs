using Altinn.Authorization.ModelUtils;
using Altinn.Authorization.ModelUtils.Swashbuckle;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding ModelUtils.Swashbuckle services to an <see cref="IServiceCollection"/>.
/// </summary>
public static class ModelUtilsSwashbuckleDependencyInjectionExtensions
{
    /// <summary>
    /// Adds support for <see cref="NonExhaustiveEnum{T}"/> for Swagger/OpenAPI.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddExtensibleEnumSwaggerSupport(this IServiceCollection services)
    {
        services.TryAddSingleton<NonExhaustiveEnumSchemaFilter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<SwaggerGenOptions>, SwaggerGenConfigAddSchemaFilter<NonExhaustiveEnumSchemaFilter>>());

        return services;
    }

    private sealed class SwaggerGenConfigAddSchemaFilter<TSchemaFilter>
        : IConfigureNamedOptions<SwaggerGenOptions>
        where TSchemaFilter : ISchemaFilter
    {
        private readonly TSchemaFilter _schemaFilter;

        public SwaggerGenConfigAddSchemaFilter(TSchemaFilter schemaFilter)
        {
            _schemaFilter = schemaFilter;
        }

        public void Configure(string? name, SwaggerGenOptions options)
        {
            options.SchemaGeneratorOptions.SchemaFilters.Add(_schemaFilter);
        }

        public void Configure(SwaggerGenOptions options)
            => Configure(null, options);
    }
}
