using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class JsonUtils
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };
    
    public static readonly JsonWriterOptions JsonWriterOptions = new()
    {
        Indented = true
    };
    
    public static readonly JsonReaderOptions JsonReaderOptions = default;

    public static T? Deserialize<T>(in ReadOnlySequence<byte> json)
    {
        var reader = new Utf8JsonReader(json, JsonReaderOptions);
        return JsonSerializer.Deserialize<T>(ref reader, JsonSerializerOptions);
    }

    public static void Serialize<T>(IBufferWriter<byte> writer, T value)
    {
        using var jsonWriter = new Utf8JsonWriter(writer, JsonWriterOptions);
        JsonSerializer.Serialize(jsonWriter, value, JsonSerializerOptions);
    }
}

[ExcludeFromCodeCoverage]
internal static class StreamExtensions
{
    public static ValueTask WriteAsync(this Stream stream, ReadOnlySequence<byte> data, CancellationToken cancellationToken = default)
    {
        if (data.IsSingleSegment)
        {
            return stream.WriteAsync(data.First, cancellationToken);
        }

        return WriteMany(stream, data, cancellationToken);

        static async ValueTask WriteMany(Stream stream, ReadOnlySequence<byte> data, CancellationToken cancellationToken)
        {
            foreach (var segment in data)
            {
                await stream.WriteAsync(segment, cancellationToken);
            }
        }
    }
}
