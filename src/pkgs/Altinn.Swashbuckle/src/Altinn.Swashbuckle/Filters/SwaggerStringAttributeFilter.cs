using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Swashbuckle.Filters;

[ExcludeFromCodeCoverage]
internal class SwaggerStringAttributeFilter
    : AttributeFilter<SwaggerStringAttribute>
{
    protected override void Apply(SwaggerStringAttribute attribute, IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema)
        {
            return;
        }
        // Reset the schema type to string
        // reset defaults
        openApiSchema.Properties?.Clear();
        openApiSchema.Required?.Clear();
        openApiSchema.AdditionalPropertiesAllowed = false;
        openApiSchema.AdditionalProperties = null;
        openApiSchema.Type = JsonSchemaType.String;
        openApiSchema.Format = attribute.Format;
        openApiSchema.Pattern = attribute.Pattern;
    }
}
