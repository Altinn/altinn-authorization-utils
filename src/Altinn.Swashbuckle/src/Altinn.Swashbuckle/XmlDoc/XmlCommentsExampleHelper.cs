using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Altinn.Swashbuckle.XmlDoc;

internal static class XmlCommentsExampleHelper
{
    public static JsonNode? Create(
        SchemaRepository schemaRepository,
        IOpenApiSchema schema,
        string exampleString)
    {
        var isStringType =
            (schema.Type & JsonSchemaType.String) == JsonSchemaType.String &&
            !string.Equals(exampleString, "null");

        var exampleAsJson = isStringType
                ? JsonSerializer.Serialize(exampleString)
                : exampleString;

        try
        {
            return JsonNode.Parse(exampleAsJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
