using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

/// <summary>
/// A default implementation of <see cref="JsonConverter{T}"/> for <see cref="IUrn{T}"/>.
/// 
/// It supports reading URNs as strings and as objects with a type and value property, and writing URNs as strings.
/// </summary>
public sealed class UrnJsonConverter
    : BaseUrnJsonConverterFactory
{
    protected override BaseUrnJsonConverter<T> CreateConverter<T>()
        => new UrnJsonConverter<T>();
}

/// <summary>
/// A default implementation of <see cref="JsonConverter{T}"/> for <see cref="IUrn{T}"/>.
/// 
/// It supports reading URNs as strings and as objects with a type and value property, and writing URNs as strings.
/// </summary>
/// <typeparam name="T">The URN type.</typeparam>
public sealed class UrnJsonConverter<T>
    : BaseUrnJsonConverter<T>
    where T : IUrn<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType is JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
        {
            return ReadUrnString(ref reader);
        }

        if (reader.TokenType is JsonTokenType.StartObject)
        {
            return ReadTypeValueObject(ref reader);
        }

        throw new JsonException($"Expected {Name} as string or object, but got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        WriteUrnString(writer, value);
    }
}
