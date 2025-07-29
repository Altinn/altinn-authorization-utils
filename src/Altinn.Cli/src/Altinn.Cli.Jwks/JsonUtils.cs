using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class JsonUtils
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = true,
    };

    public static T? Deserialize<T>(in ReadOnlySequence<byte> json)
    {
        var reader = new Utf8JsonReader(json);
        return JsonSerializer.Deserialize<T>(ref reader, SerializerOptions);
    }

    public static void Serialize<T>(IBufferWriter<byte> writer, T value)
    {
        using var jsonWriter = new Utf8JsonWriter(writer, WriterOptions);
        JsonSerializer.Serialize(jsonWriter, value, SerializerOptions);
    }
}
