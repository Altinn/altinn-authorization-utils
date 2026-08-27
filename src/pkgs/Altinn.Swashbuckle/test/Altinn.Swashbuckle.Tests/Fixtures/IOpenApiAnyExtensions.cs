using System.Text.Json.Nodes;


namespace Altinn.Swashbuckle.Tests.Fixtures;

public static class IOpenApiAnyExtensions
{
    public static string ToJson(this JsonNode openApiAny)
        => openApiAny.ToJsonString();
}
