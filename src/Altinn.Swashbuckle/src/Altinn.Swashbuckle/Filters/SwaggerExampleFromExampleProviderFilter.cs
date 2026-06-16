using Altinn.Swashbuckle.Examples;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Swashbuckle.Filters;

[ExcludeFromCodeCoverage]
internal class SwaggerExampleFromExampleProviderFilter
    : AttributeFilter<SwaggerExampleFromExampleProviderAttribute>
{
    private readonly OpenApiExampleProvider _provider;

    public SwaggerExampleFromExampleProviderFilter(OpenApiExampleProvider provider)
    {
        _provider = provider;
    }

    protected override void Apply(SwaggerExampleFromExampleProviderAttribute attribute, IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema)
        {
            return;
        }
        openApiSchema.Example ??= _provider.GetExample(context.Type)?.FirstOrDefault();
    }
}
