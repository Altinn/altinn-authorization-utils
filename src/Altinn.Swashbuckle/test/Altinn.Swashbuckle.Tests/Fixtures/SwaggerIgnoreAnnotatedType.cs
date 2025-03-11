using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Swashbuckle.Tests.Fixtures;

public class SwaggerIgnoreAnnotatedType
{
    public string? NotIgnoredString { get; set; }

    [SwaggerIgnore]
    public string? IgnoredString { get; set; }

    [SwaggerIgnore]
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? IgnoredExtensionData { get; set; }
}
