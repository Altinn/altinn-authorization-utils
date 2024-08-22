using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Swashbuckle.Filters;

[ExcludeFromCodeCoverage]
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
