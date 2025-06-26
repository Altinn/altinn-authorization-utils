using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Diagnostics;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Json;

internal sealed class JsonExtensionDataBuilder
    : IDisposable
{
    private readonly ArrayPoolWriter<byte> _buffer;
    private readonly Utf8JsonWriter _writer;

    public JsonExtensionDataBuilder()
    {
        _buffer = new(1024);
        _writer = new(_buffer, new JsonWriterOptions { SkipValidation = true });
        _writer.WriteStartObject();
    }

    public JsonElement Build()
    {
        _writer.WriteEndObject();
        _writer.Flush();

        var reader = new Utf8JsonReader(_buffer.WrittenSpan);
        return JsonElement.ParseValue(ref reader);
    }

    public void AddProperty(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            ThrowHelper.ThrowArgumentException("Expected PropertyName token type.", nameof(reader));
        }

        WritePropertyName(ref reader);

        if (!reader.Read())
        {
            ThrowHelper.ThrowArgumentException("Expected a value after the property name.", nameof(reader));
        }

        WriteValue(ref reader);
    }

    private void WritePropertyName(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.PropertyName);

        if (!reader.HasValueSequence && !reader.ValueIsEscaped)
        {
            _writer.WritePropertyName(reader.ValueSpan);
            return;
        }

        WritePropertyNameSlow(ref reader);
    }

    private void WritePropertyNameSlow(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.PropertyName);

        var length = reader.HasValueSequence
            ? checked((int)reader.ValueSequence.Length)
            : reader.ValueSpan.Length;

        var buffer = ArrayPool<char>.Shared.Rent(length);
        try
        {
            var written = reader.CopyString(buffer);
            Debug.Assert(written <= length, "The buffer should be large enough to hold the property name.");

            var propertyNameSpan = buffer.AsSpan(0, written);
            _writer.WritePropertyName(propertyNameSpan);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer, clearArray: true);
        }
    }

    private void WriteValue(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
                WriteArray(ref reader);
                return;

            case JsonTokenType.StartObject:
                WriteObject(ref reader);
                return;

            case JsonTokenType.String:
                WriteString(ref reader);
                return;

            case JsonTokenType.Number:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.Null:
                WriteRawValue(ref reader);
                return;

            default:
                ThrowHelper.ThrowInvalidOperationException($"Unexpected token type: {reader.TokenType}");
                return;
        }
    }

    private void WriteArray(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.StartArray);

        _writer.WriteStartArray();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            WriteValue(ref reader);
        }
        _writer.WriteEndArray();

        Debug.Assert(reader.TokenType == JsonTokenType.EndArray);
    }

    private void WriteObject(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.StartObject);

        _writer.WriteStartObject();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            AddProperty(ref reader);
        }
        _writer.WriteEndObject();

        Debug.Assert(reader.TokenType == JsonTokenType.EndObject);
    }

    private void WriteString(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.String);

        if (reader.HasValueSequence)
        {
            _writer.WriteRawValue(QuoteWrap(reader.ValueSequence), skipInputValidation: true);
        }
        else if (reader.ValueIsEscaped)
        {
            _writer.WriteRawValue(QuoteWrap(reader.ValueSpan), skipInputValidation: true);
        }
        else
        {
            _writer.WriteStringValue(reader.ValueSpan);
        }
    }

    private void WriteRawValue(ref Utf8JsonReader reader)
    {
        if (reader.HasValueSequence)
        {
            _writer.WriteRawValue(reader.ValueSequence, skipInputValidation: true);
        }
        else
        {
            _writer.WriteRawValue(reader.ValueSpan, skipInputValidation: true);
        }
    }

    private static ReadOnlyMemory<byte> Utf8Quote = (byte[])[.. "\""u8];

    private ReadOnlySequence<byte> QuoteWrap(ReadOnlySpan<byte> inner)
    {
        var start = new QuoteReadOnlySegment(Utf8Quote, 0);
        var current = start;

        var next = new QuoteReadOnlySegment((byte[])[.. inner], current.RunningIndex + current.Memory.Length);
        current.NextSegment = next;
        current = next;

        var end = new QuoteReadOnlySegment(Utf8Quote, current.RunningIndex + current.Memory.Length);
        current.NextSegment = end;

        return new ReadOnlySequence<byte>(start, 0, end, end.Memory.Length);
    }

    private ReadOnlySequence<byte> QuoteWrap(ReadOnlySequence<byte> inner)
    {
        var start = new QuoteReadOnlySegment(Utf8Quote, 0);
        var current = start;

        foreach (var segment in inner)
        {
            var next = new QuoteReadOnlySegment(segment, current.RunningIndex + current.Memory.Length);
            current.NextSegment = next;

            current = next;
        }
        
        var end = new QuoteReadOnlySegment(Utf8Quote, current.RunningIndex + current.Memory.Length);
        current.NextSegment = end;

        return new ReadOnlySequence<byte>(start, 0, end, end.Memory.Length);
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }

    private sealed class QuoteReadOnlySegment
        : ReadOnlySequenceSegment<byte>
    {
        public QuoteReadOnlySegment(ReadOnlyMemory<byte> data, long offset) 
        {
            Memory = data;
            RunningIndex = offset;
        }

        public QuoteReadOnlySegment NextSegment
        {
            set => Next = value;
        }
    }
}
