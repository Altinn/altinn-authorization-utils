using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

/// <summary>
/// An implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="UrnJsonTypeValue"/>.
/// </summary>
internal sealed class TypeValueObjectKeyValueUrnJsonConverter
    : JsonConverter<UrnJsonTypeValue>
{
    /// <inheritdoc/>
    public override UrnJsonTypeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.StartObject)
        {
            return UrnJsonConverter.ReadGenericTypeValueObject(ref reader);
        }

        throw new JsonException($"Expected type-value object, but got {reader.TokenType}");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, UrnJsonTypeValue value, JsonSerializerOptions options)
    {
        UrnJsonConverter.WriteUrnTypeValueObject(writer, value.Value);
    }
}
