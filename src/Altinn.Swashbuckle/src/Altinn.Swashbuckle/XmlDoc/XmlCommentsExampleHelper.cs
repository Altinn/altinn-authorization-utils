using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace Altinn.Swashbuckle.XmlDoc;

internal static class XmlCommentsExampleHelper
{
    public static IOpenApiAny Create(
        SchemaRepository schemaRepository,
        OpenApiSchema schema,
        string exampleString)
    {
        var isStringType =
            (schema?.ResolveType(schemaRepository) == "string") &&
            !string.Equals(exampleString, "null");

        var exampleAsJson = isStringType
                ? JsonSerializer.Serialize(exampleString)
                : exampleString;

        var example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);

        return example;
    }
}
