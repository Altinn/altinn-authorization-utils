using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.Filters;

internal class SchemaFilterAttributeFilter
    : AttributeFilter<ISchemaFilter>
{
    protected override void Apply(ISchemaFilter attribute, OpenApiSchema schema, SchemaFilterContext context)
    {
        attribute.Apply(schema, context);
    }
}
