﻿using Altinn.Authorization.ModelUtils;
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
    /// Adds Swagger support for authorization model utilities to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddAuthorizationModelUtilsSwaggerSupport(this IServiceCollection services)
    {
        services.AddExtensibleEnumSwaggerSupport();
        services.AddFieldValueRecordSwaggerSupport();
        services.AddPolymorphicFieldValueRecordSwaggerSupport();
        
        return services;
    }

    /// <summary>
    /// Adds support for <see cref="NonExhaustiveEnum{T}"/> for Swagger/OpenAPI.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddExtensibleEnumSwaggerSupport(this IServiceCollection services)
    {
        services.AddXmlDocProvider();
        services.TryAddSingleton<NonExhaustiveEnumSchemaFilter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<SwaggerGenOptions>, SwaggerGenConfigAddSchemaFilter<NonExhaustiveEnumSchemaFilter>>());

        return services;
    }

    /// <summary>
    /// Adds support for field-value-records for Swagger/OpenAPI.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddFieldValueRecordSwaggerSupport(this IServiceCollection services)
    {
        services.TryAddSingleton<FieldValueRecordSchemaFilter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<SwaggerGenOptions>, SwaggerGenConfigAddSchemaFilter<FieldValueRecordSchemaFilter>>());

        return services;
    }

    /// <summary>
    /// Adds support for polymorphic field-value-records for Swagger/OpenAPI.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddPolymorphicFieldValueRecordSwaggerSupport(this IServiceCollection services)
    {
        services.TryAddSingleton<PolymorphicFieldValueRecordSchemaFilter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<SwaggerGenOptions>, SwaggerGenConfigAddSchemaFilter<PolymorphicFieldValueRecordSchemaFilter>>());

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
