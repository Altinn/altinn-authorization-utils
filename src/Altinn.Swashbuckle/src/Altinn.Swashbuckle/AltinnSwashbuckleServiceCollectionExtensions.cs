﻿using Altinn.Swashbuckle.Examples;
using Altinn.Swashbuckle.Filters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding example-data to the generated OpenAPI spec.
/// </summary>
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
        services.AddSingleton<SchemaFilterAttributeFilter>();

        services.AddOptions<SwaggerGenOptions>()
            .Configure((SwaggerGenOptions options, SchemaFilterAttributeFilter filter) =>
            {
                options.SchemaGeneratorOptions.SchemaFilters.Add(filter);
            });

        return services;
    }
}
