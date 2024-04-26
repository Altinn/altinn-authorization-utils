using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
public sealed class SwaggerStringAttribute
    : Attribute
    , ISchemaFilter
{
    public string? Format { get; set; }

    public string? Pattern { get; set; }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Reset the schema type to string
        // reset defaults
        schema.Properties.Clear();
        schema.Required.Clear();
        schema.AdditionalPropertiesAllowed = false;
        schema.AdditionalProperties = null;
        schema.Type = "string";
        schema.Format = Format;
        schema.Pattern = Pattern;
    }
}
