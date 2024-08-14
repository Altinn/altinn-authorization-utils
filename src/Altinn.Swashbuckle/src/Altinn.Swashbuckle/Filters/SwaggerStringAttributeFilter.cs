using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.Filters;

internal class SwaggerStringAttributeFilter
    : AttributeFilter<SwaggerStringAttribute>
{
    protected override void Apply(SwaggerStringAttribute attribute, OpenApiSchema schema, SchemaFilterContext context)
    {
        // Reset the schema type to string
        // reset defaults
        schema.Properties.Clear();
        schema.Required.Clear();
        schema.AdditionalPropertiesAllowed = false;
        schema.AdditionalProperties = null;
        schema.Type = "string";
        schema.Format = attribute.Format;
        schema.Pattern = attribute.Pattern;
    }
}
