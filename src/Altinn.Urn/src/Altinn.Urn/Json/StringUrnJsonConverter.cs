using System.Text.Json;

namespace Altinn.Urn.Json;

/// <summary>
/// A implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="IUrn{T}"/>.
/// 
/// It supports reading URNs as strings, and writing URNs as strings.
/// </summary>
public sealed class StringUrnJsonConverter
    : BaseUrnJsonConverterFactory
{
    protected override BaseUrnJsonConverter<T> CreateConverter<T>()
        => new StringUrnJsonConverter<T>();
}

/// <summary>
/// A implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="IUrn{T}"/>.
/// 
/// It supports reading URNs as strings, and writing URNs as strings.
/// </summary>
/// <typeparam name="T">The URN type.</typeparam>
public sealed class StringUrnJsonConverter<T>
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

        throw new JsonException($"Expected {Name} as string, but got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        WriteUrnString(writer, value);
    }
}
