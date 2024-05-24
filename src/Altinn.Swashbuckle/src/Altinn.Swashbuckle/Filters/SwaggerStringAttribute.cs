using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.Filters;

/// <summary>
/// Attribute for specifying that a type should be represented as a string in the OpenAPI schema.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
public sealed class SwaggerStringAttribute
    : Attribute
    , ISchemaFilter
{
    /// <summary>
    /// Gets or sets the (optional) format of the string.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the (optional) pattern of the string.
    /// </summary>
    public string? Pattern { get; set; }

    /// <inheritdoc/>
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
