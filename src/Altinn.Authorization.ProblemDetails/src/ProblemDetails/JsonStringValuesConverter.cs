using Microsoft.Extensions.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

internal class JsonStringValuesConverter
    : JsonConverter<StringValues>
{
    public override StringValues Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var array = JsonSerializer.Deserialize<string[]>(ref reader, options);
        return new StringValues(array);
    }

    public override void Write(Utf8JsonWriter writer, StringValues value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var v in value)
        {
            writer.WriteStringValue(v);
        }
        writer.WriteEndArray();
    }
}
