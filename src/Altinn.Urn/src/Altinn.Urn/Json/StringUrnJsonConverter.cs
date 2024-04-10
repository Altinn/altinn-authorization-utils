using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

/// <summary>
/// A implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="IKeyValueUrn{TSelf}"/>.
/// 
/// It supports reading URNs as strings, and writing URNs as strings.
/// </summary>
internal sealed class StringUrnJsonConverter
    : BaseUrnJsonWrapperConverterFactory
{
    protected override JsonConverter<TWrapper> CreateConverter<TWrapper, TUrn>()
        => new StringUrnJsonConverter<TWrapper, TUrn>();
}

/// <summary>
/// A implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="IKeyValueUrn{TSelf}"/>.
/// 
/// It supports reading URNs as strings, and writing URNs as strings.
/// </summary>
/// <typeparam name="TUrn">The URN type.</typeparam>
internal sealed class StringUrnJsonConverter<TWrapper, TUrn>
    : JsonConverter<TWrapper>
    where TWrapper : IUrnJsonWrapper<TWrapper, TUrn>
    where TUrn : IKeyValueUrn<TUrn>
{
    private static readonly string Name = typeof(TUrn).Name;
    private static readonly string InvalidUrn = $"Invalid {Name}";

    public override TWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return default(TUrn);
        }

        if (reader.TokenType is JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
        {
            return UrnJsonConverter.ReadUrnString<TUrn>(ref reader, Name, InvalidUrn);
        }

        throw new JsonException($"Expected {Name} as string, but got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, TWrapper value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        UrnJsonConverter.WriteUrnString(writer, value.Value);
    }
}
