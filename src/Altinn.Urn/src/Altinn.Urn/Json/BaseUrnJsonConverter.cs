using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

public abstract class BaseUrnJsonConverter<T>
    : JsonConverter<T>
    where T : IUrn<T>
{
    private static readonly JsonEncodedText TypePropertyName = JsonEncodedText.Encode("type");
    private static readonly JsonEncodedText ValuePropertyName = JsonEncodedText.Encode("value");

    protected static readonly string Name = typeof(T).Name;
    protected static readonly string InvalidUrn = $"Invalid {Name}";

    protected static T ReadUrnString(ref Utf8JsonReader reader)
    {
        var urn = reader.GetString();
        if (urn is null)
        {
            throw new JsonException($"Expected {Name} as string, but got null");
        }

        if (!T.TryParse(urn, out var result))
        {
            throw new JsonException(InvalidUrn);
        }

        return result;
    }

    protected static void WriteUrnString(Utf8JsonWriter writer, T value)
    {
        writer.WriteStringValue(value.Urn);
    }

    protected static T ReadTypeValueObject(ref Utf8JsonReader reader)
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
            throw new JsonException(InvalidUrn);
        }

        return result;
    }

    protected static void WriteUrnTypeValueObject(Utf8JsonWriter writer, T value)
    {
        writer.WriteStartObject();
        writer.WriteString(TypePropertyName, value.PrefixSpan);
        writer.WriteString(ValuePropertyName, value.ValueSpan);
        writer.WriteEndObject();
    }
}

//public sealed class UrnJsonConverter<T> 
//    : JsonConverter<T>
//    where T : IUrn<T>
//{
//    private static readonly JsonEncodedText TypePropertyName = JsonEncodedText.Encode("type");
//    private static readonly JsonEncodedText ValuePropertyName = JsonEncodedText.Encode("value");

//    private static readonly string Name = typeof(T).Name;
//    private static readonly string InvalidUrn = $"Invalid {Name}";

//    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        if (reader.TokenType is JsonTokenType.Null)
//        {
//            return default;
//        }

//        if (reader.TokenType is JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
//        {
//            return ReadString(ref reader);
//        }

//        if (reader.TokenType is JsonTokenType.StartObject)
//        {
//            return ReadObject(ref reader);
//        }

//        throw new JsonException($"Expected {Name} as string or object, but got {reader.TokenType}");
//    }

//    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
//    {
//        writer.WriteStringValue(value.Urn);
//    }

//    private static T ReadString(ref Utf8JsonReader reader)
//    {
//        var urn = reader.GetString();
//        if (urn is null)
//        {
//            throw new JsonException($"Expected {Name} as string, but got null");
//        }

//        if (!T.TryParse(urn, out var result))
//        {
//            throw new JsonException(InvalidUrn);
//        }

//        return result;
//    }

//    private static T ReadObject(ref Utf8JsonReader reader)
//    {
//        string? type = null;
//        string? value = null;

//        while (reader.Read())
//        {
//            if (reader.TokenType is JsonTokenType.EndObject)
//            {
//                break;
//            }

//            if (reader.TokenType is JsonTokenType.PropertyName)
//            {
//                if (reader.ValueTextEquals(TypePropertyName.EncodedUtf8Bytes))
//                {
//                    reader.Read();
//                    type = reader.GetString();
//                } 
//                else if (reader.ValueTextEquals(ValuePropertyName.EncodedUtf8Bytes))
//                {
//                    reader.Read();
//                    value = reader.GetString();
//                }
//                else
//                {
//                    throw new JsonException($"Unexpected property {reader.GetString()}");
//                }
//            }
//        }

//        if (type is null)
//        {
//            throw new JsonException($"Missing type property");
//        }

//        if (value is null)
//        {
//            throw new JsonException($"Missing value property");
//        }

//        var urn = $"{type}:{value}";
//        if (!T.TryParse(urn, out var result))
//        {
//            throw new JsonException(InvalidUrn);
//        }

//        return result;
//    }
//}
