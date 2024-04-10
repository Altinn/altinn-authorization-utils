using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

/// <summary>
/// A default implementation of <see cref="JsonConverter{T}"/> for <see cref="IKeyValueUrn{TSelf}"/>.
/// 
/// It supports reading URNs as strings and as objects with a type and value property, and writing URNs as strings.
/// </summary>
/// <typeparam name="T">The URN type.</typeparam>
public sealed class UrnJsonConverter<T>
    : JsonConverter<T>
    where T : IKeyValueUrn<T>
{
    private static readonly string Name = typeof(T).Name;
    private static readonly string InvalidUrn = $"Invalid {Name}";

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType is JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
        {
            return UrnJsonConverter.ReadUrnString<T>(ref reader, Name, InvalidUrn);
        }

        if (reader.TokenType is JsonTokenType.StartObject)
        {
            return UrnJsonConverter.ReadTypeValueObject<T>(ref reader, Name, InvalidUrn);
        }

        throw new JsonException($"Expected {Name} as string or object, but got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        UrnJsonConverter.WriteUrnString(writer, value);
    }
}

internal static class UrnJsonConverter
{
    private static readonly JsonEncodedText TypePropertyName = JsonEncodedText.Encode("type");
    private static readonly JsonEncodedText ValuePropertyName = JsonEncodedText.Encode("value");

    public static T ReadUrnString<T>(ref Utf8JsonReader reader, string name, string invalidUrn)
        where T : IKeyValueUrn<T>
    {
        var urn = reader.GetString();
        if (urn is null)
        {
            throw new JsonException($"Expected {name} as string, but got null");
        }

        if (!T.TryParse(urn, out var result))
        {
            throw new JsonException(invalidUrn);
        }

        return result;
    }

    public static void WriteUrnString<T>(Utf8JsonWriter writer, T value)
        where T : IKeyValueUrn
    {
        writer.WriteStringValue(value.Urn);
    }

    public static T ReadTypeValueObject<T>(ref Utf8JsonReader reader, string name, string invalidUrn)
        where T : IKeyValueUrn<T>
    {
        string? type = null;
        string? value = null;

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType is JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(TypePropertyName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    type = reader.GetString();
                }
                else if (reader.ValueTextEquals(ValuePropertyName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    value = reader.GetString();
                }
                else
                {
                    throw new JsonException($"Unexpected property {reader.GetString()}");
                }
            }
        }

        if (type is null)
        {
            throw new JsonException($"Missing type property");
        }

        if (value is null)
        {
            throw new JsonException($"Missing value property");
        }

        var urn = $"{type}:{value}";
        if (!T.TryParse(urn, out var result))
        {
            throw new JsonException(invalidUrn);
        }

        return result;
    }

    public static void WriteUrnTypeValueObject<T>(Utf8JsonWriter writer, T value)
        where T : IKeyValueUrn
    {
        writer.WriteStartObject();
        writer.WriteString(TypePropertyName, value.PrefixSpan);
        writer.WriteString(ValuePropertyName, value.ValueSpan);
        writer.WriteEndObject();
    }
}
