using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal sealed class JsonWithCommentsReader
{
    internal static JsonWithCommentsNode Read(ref Utf8JsonReader reader)
    {
        var inst = new JsonWithCommentsReader();
        return inst.ReadOuter(ref reader);
    }

    private ImmutableArray<string>.Builder? _comments;

    public JsonWithCommentsNode ReadOuter(ref Utf8JsonReader reader)
    {
        if (!reader.Read())
        {
            ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
        }

        var root = ReadValue(ref reader);

        ReadComments(ref reader);
        var trailingComments = GetComments();
        return root.WithTrailingCommentLines(trailingComments);
    }

    private JsonWithCommentsNode ReadValue(ref Utf8JsonReader reader)
    {
        if (!ReadComments(ref reader))
        {
            ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
        }

        return reader.TokenType switch
        {
            JsonTokenType.StartArray => ReadArray(ref reader),
            JsonTokenType.StartObject => ReadObject(ref reader),
            JsonTokenType.String => ReadString(ref reader),
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.True or JsonTokenType.False => ReadBool(ref reader),
            JsonTokenType.Null => ReadNull(ref reader),
            _ => ThrowHelper.ThrowArgumentException<JsonWithCommentsNode>(nameof(reader), $"Unexpected JSON token: {reader.TokenType}."),
        };
    }

    private JsonWithCommentsNode ReadNull(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.Null);

        var leadingComments = GetComments();
        var node = JsonNullNode.Instance.WithLeadingCommentLines(leadingComments);

        reader.Read();
        return node;
    }

    private JsonWithCommentsNode ReadBool(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType is JsonTokenType.True or JsonTokenType.False);

        var leadingComments = GetComments();
        var node = (reader.GetBoolean() ? JsonBoolNode.True : JsonBoolNode.False)
            .WithLeadingCommentLines(leadingComments);

        reader.Read();
        return node;
    }

    private JsonWithCommentsNode ReadString(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.String);

        var leadingComments = GetComments();
        var node = new JsonStringNode(reader.GetString()!).WithLeadingCommentLines(leadingComments);

        reader.Read();
        return node;
    }

    private JsonWithCommentsNode ReadNumber(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.Number);

        var leadingComments = GetComments();
        var numberText = reader.HasValueSequence ? ToString(reader.ValueSequence) : ToString(reader.ValueSpan);
        var node = new JsonNumberNode(numberText).WithLeadingCommentLines(leadingComments);

        reader.Read();
        return node;
    }

    private JsonWithCommentsNode ReadArray(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.StartArray);

        var leadingComments = GetComments();
        var builder = ImmutableArray.CreateBuilder<JsonWithCommentsNode>();

        if (!reader.Read())
        {
            ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
        }

        if (!ReadComments(ref reader))
        {
            ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
        }

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            var child = ReadValue(ref reader);
            builder.Add(child);

            if (!ReadComments(ref reader))
            {
                ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
            }
        }

        var closingComments = GetComments();
        reader.Read();

        return new JsonArrayNode(builder.DrainToImmutable())
            .WithClosingCommentLines(closingComments)
            .WithLeadingCommentLines(leadingComments);
    }

    private JsonWithCommentsNode ReadObject(ref Utf8JsonReader reader)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.StartObject);

        var leadingComments = GetComments();
        var builder = ImmutableArray.CreateBuilder<JsonPropertyNode>();

        if (!reader.Read())
        {
            ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
        }

        if (!ReadComments(ref reader))
        {
            ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
        }

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                ThrowHelper.ThrowArgumentException(nameof(reader), $"Unexpected JSON token: {reader.TokenType}. Expected a property name.");
            }

            var prePropComments = GetComments();
            var propertyName = reader.GetString()!;
            if (!reader.Read())
            {
                ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
            }

            if (!ReadComments(ref reader))
            {
                ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
            }

            var inPropComments = GetComments();
            var valueNode = ReadValue(ref reader);

            var propertyNode = (JsonPropertyNode)new JsonPropertyNode(propertyName, valueNode)
                .WithLeadingCommentLines([.. prePropComments, .. inPropComments]);

            builder.Add(propertyNode);

            if (!ReadComments(ref reader))
            {
                ThrowHelper.ThrowArgumentException(nameof(reader), "Unexpected end of JSON input.");
            }
        }

        var closingComments = GetComments();
        reader.Read();

        return new JsonObjectNode(builder.DrainToImmutable())
            .WithClosingCommentLines(closingComments)
            .WithLeadingCommentLines(leadingComments);
    }

    private bool ReadComments(ref Utf8JsonReader reader)
    {
        while (reader.TokenType == JsonTokenType.Comment)
        {
            _comments ??= ImmutableArray.CreateBuilder<string>();
            var lines = reader.GetComment().Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            _comments.AddRange(lines);

            if (!reader.Read())
            {
                return false;
            }
        }

        return true;
    }

    private ImmutableArray<string> GetComments()
    {
        if (_comments is null)
        {
            return [];
        }

        return _comments.DrainToImmutable();
    }

    private static string ToString(ReadOnlySpan<byte> utf8Bytes)
        => Encoding.UTF8.GetString(utf8Bytes);

    private static string ToString(ReadOnlySequence<byte> utf8Bytes)
        => Encoding.UTF8.GetString(utf8Bytes);
}
