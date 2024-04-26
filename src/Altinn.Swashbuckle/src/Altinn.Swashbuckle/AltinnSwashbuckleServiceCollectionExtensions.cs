using Altinn.Swashbuckle.Examples;
using Altinn.Swashbuckle.Filters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection;

public static class AltinnSwashbuckleServiceCollectionExtensions
{
    public static OptionsBuilder<ExampleDataOptions> AddOpenApiExampleProvider(this IServiceCollection services)
    {
        var builder = services.AddExampleDataOptions();

        services.TryAddSingleton<OpenApiExampleProvider>();

        return builder;
    }

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
