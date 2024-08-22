using Altinn.Swashbuckle.Examples;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.Filters;

internal class SwaggerExampleFromExampleProviderFilter
    : AttributeFilter<SwaggerExampleFromExampleProviderAttribute>
{
    private readonly OpenApiExampleProvider _provider;

    public SwaggerExampleFromExampleProviderFilter(OpenApiExampleProvider provider)
    {
        _provider = provider;
    }

    protected override void Apply(SwaggerExampleFromExampleProviderAttribute attribute, OpenApiSchema schema, SchemaFilterContext context)
    {
        schema.Example ??= _provider.GetExample(context.Type)?.FirstOrDefault();
    }
}
