using CommunityToolkit.Diagnostics;
using Nerdbank.Streams;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class JsonUtils
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
#if DEBUG
        WriteIndented = true,
#endif
    };

    public static readonly JsonWriterOptions WriterOptions = new()
    {
#if DEBUG
        Indented = true,
#endif
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

    public static T DeepClone<T>(T value)
    {
        using var seq = new Sequence<byte>(ArrayPool<byte>.Shared);
        Serialize(seq, value);
        return Deserialize<T>(seq.AsReadOnlySequence) 
            ?? ThrowHelper.ThrowInvalidOperationException<T>("Failed to deserialize cloned object.");
    }
}
