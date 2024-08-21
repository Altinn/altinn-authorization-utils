using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Altinn.Urn.Json;

/// <summary>
/// An implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="IKeyValueUrn{TSelf}"/>.
/// 
/// It supports reading URNs as objects with a type and value property, and writing URNs as objects with a type and value property.
/// </summary>
internal sealed class TypeValueObjectUrnJsonConverter
    : BaseUrnJsonWrapperConverterFactory
{
    protected override JsonConverter<TWrapper> CreateConverter<TWrapper, TUrn>()
        => new TypeValueObjectUrnJsonConverter<TWrapper, TUrn>();
}

/// <summary>
/// A implementation of <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="IKeyValueUrn{TSelf}"/>.
/// 
/// It supports reading URNs as objects with a type and value property, and writing URNs as objects with a type and value property.
/// </summary>
/// <typeparam name="TWrapper">A utility wrapper type.</typeparam>
/// <typeparam name="TUrn">The URN type.</typeparam>
internal sealed class TypeValueObjectUrnJsonConverter<TWrapper, TUrn>
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

        if (reader.TokenType is JsonTokenType.StartObject)
        {
            return UrnJsonConverter.ReadTypeValueObject<TUrn>(ref reader, Name, InvalidUrn);
        }

        throw new JsonException($"Expected {Name} as object, but got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, TWrapper value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        UrnJsonConverter.WriteUrnTypeValueObject(writer, value.Value);
    }
}
