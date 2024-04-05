using System.Text.Json;

namespace Altinn.Urn.Json;

/// <summary>
/// A implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="IUrn{T}"/>.
/// 
/// It supports reading URNs as objects with a type and value property, and writing URNs as objects with a type and value property.
/// </summary>
public sealed class TypeValueObjectUrnJsonConverter
    : BaseUrnJsonConverterFactory
{
    protected override BaseUrnJsonConverter<T> CreateConverter<T>()
        => new TypeValueObjectUrnJsonConverter<T>();
}

/// <summary>
/// A implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="IUrn{T}"/>.
/// 
/// It supports reading URNs as objects with a type and value property, and writing URNs as objects with a type and value property.
/// </summary>
/// <typeparam name="T">The URN type.</typeparam>
public sealed class TypeValueObjectUrnJsonConverter<T>
    : BaseUrnJsonConverter<T>
    where T : IUrn<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType is JsonTokenType.StartObject)
        {
            return ReadTypeValueObject(ref reader);
        }

        throw new JsonException($"Expected {Name} as object, but got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        WriteUrnTypeValueObject(writer, value);
    }
}
