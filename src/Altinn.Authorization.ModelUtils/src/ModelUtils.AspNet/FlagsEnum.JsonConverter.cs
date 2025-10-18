using Altinn.Authorization.ModelUtils.EnumUtils;
using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.AspNet;

public sealed partial class FlagsEnum
{
    internal sealed class JsonConverterProvider
        : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => IsFlagsEnumModelType(typeToConvert, out _);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (!IsFlagsEnumModelType(typeToConvert, out var enumType))
            {
                ThrowHelper.ThrowInvalidOperationException($"Type {typeToConvert} is not a {nameof(FlagsEnum)}<> type");
            }

            var converterType = typeof(ConcreteJsonConverter<>).MakeGenericType(enumType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    internal sealed class ConcreteJsonConverter<TEnum>
        : JsonConverter<FlagsEnum<TEnum>>
        where TEnum : struct, Enum
    {
        public override FlagsEnum<TEnum> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                return ReadArray(ref reader, options);
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return ReadString(ref reader, options);
            }

            throw new JsonException($"Expected array or string, got {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, FlagsEnum<TEnum> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var variantString in FlagsEnum<TEnum>.Model.GetComponentStrings(value))
            {
                writer.WriteStringValue(variantString);
            }
            writer.WriteEndArray();
        }

        private TEnum ReadString(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var length = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
            var buffer = ArrayPool<char>.Shared.Rent(length * 2);
            try
            {
                var strLen = reader.CopyString(buffer);
                if (!FlagsEnum<TEnum>.Model.TryParse(buffer.AsSpan(0, strLen), out var value))
                {
                    throw new JsonException($"Invalid {typeof(TEnum)} flags value");
                }

                return value;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer, clearArray: true);
            }
        }

        private TEnum ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            TEnum value = default;

            Debug.Assert(reader.TokenType == JsonTokenType.StartArray);
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException($"Expected string, got {reader.TokenType}");
                }

                //value |= ReadString(ref reader, options);
                var itemValue = ReadString(ref reader, options);
                value = value.BitwiseOr(itemValue);
            }

            return value;
        }
    }
}
