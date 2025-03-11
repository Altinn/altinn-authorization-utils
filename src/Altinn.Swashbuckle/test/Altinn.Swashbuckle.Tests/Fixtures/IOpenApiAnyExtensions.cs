using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Writers;

namespace Altinn.Swashbuckle.Tests.Fixtures;

public static class IOpenApiAnyExtensions
{
    public static string ToJson(this IOpenApiAny openApiAny)
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);

        openApiAny.Write(jsonWriter, OpenApiSpecVersion.OpenApi3_0);

        return stringWriter.ToString();
    }
}
