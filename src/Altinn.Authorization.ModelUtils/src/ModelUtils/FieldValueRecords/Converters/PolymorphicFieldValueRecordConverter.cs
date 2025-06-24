using Altinn.Authorization.ModelUtils.FieldValueRecords.Json;
using Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;
using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

internal sealed class PolymorphicFieldValueRecordConverter<T, TDiscriminator>
    : JsonConverter<T>
    , IPolymorphicFieldValueRecordJsonConverter
    , IGenericJsonConverter
    where T : class
    where TDiscriminator : struct, Enum
{
    private readonly IPolymorphicFieldValueRecordModel<TDiscriminator, T> _model;
    private readonly IFieldValueRecordPropertyJsonModel<T, NonExhaustiveEnum<TDiscriminator>> _discriminatorPropertyModel;
    private readonly PropertyName.Comparer _propertyNameComparer;
    private readonly FieldValueRecordBaseConverter<T>? _inner;

    public PolymorphicFieldValueRecordConverter(
        IPolymorphicFieldValueRecordModel<TDiscriminator, T> model,
        FieldValueRecordBaseConverter<T>? inner,
        JsonSerializerOptions options)
    {
        _model = model;
        _inner = inner;
        _propertyNameComparer = FieldValueRecordBaseConverter<T>.GetPropertyComparer(options);
        _discriminatorPropertyModel = FieldValueRecordPropertyJsonModel<T, NonExhaustiveEnum<TDiscriminator>>.Create(_model.DiscriminatorProperty, options);
    }

    /// <inheritdoc/>
    public IPolymorphicFieldValueRecordModel Model
        => _model;

    /// <inheritdoc/>
    string IPolymorphicFieldValueRecordJsonConverter.DiscriminatorPropertyName
        => _discriminatorPropertyModel.Name.Name;

    /// <inheritdoc/>
    bool IPolymorphicFieldValueRecordJsonConverter.TryFindPropertyModel(string name, [NotNullWhen(true)] out IFieldValueRecordPropertyModel? model)
    {
        if (_inner is null)
        {
            model = null;
            return false;
        }

        return ((IFieldValueRecordJsonConverter)_inner).TryFindPropertyModel(name, out model);
    }

    /// <inheritdoc/>
    bool IPolymorphicFieldValueRecordJsonConverter.IsDiscriminatorProperty(IFieldValueRecordPropertyModel model)
        => model.PropertyInfo == _model.DiscriminatorProperty.PropertyInfo;

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected object of type '{typeof(T).Name}' but got '{reader.TokenType}'");
        }

        FieldValue<NonExhaustiveEnum<TDiscriminator>> discriminator = default;

        char[]? propertyScratch = null;
        try
        {
            propertyScratch = ArrayPool<char>.Shared.Rent(_discriminatorPropertyModel.Name.Encoded.Value.Length);
            discriminator = FindDiscriminator(in reader, propertyScratch.AsSpan(), options);
        }
        finally
        {
            if (propertyScratch is not null)
            {
                propertyScratch.AsSpan().Clear();
                ArrayPool<char>.Shared.Return(propertyScratch);
            }
        }

        IPolymorphicFieldValueRecordModel<TDiscriminator> model = discriminator switch
        {
            { HasValue: false } or { Value.IsUnknown: true } => _model,
            { Value.Value: var discriminatorValue } when _model.TryGetDescendantModel(discriminatorValue, out var m) => m,
            _ => _model,
        };

        if (model.Type == _model.Type)
        {
            if (_inner is null)
            {
                throw new JsonException($"Property '{_discriminatorPropertyModel.Name}' does not match any known subtype of the polymorphic model '{_model.Type}'.");
            }

            // Not a subtype of the polymorphic model, so use the default converter
            return _inner.Read(ref reader, typeToConvert, options);
        }

        var converter = options.GetConverter(model.Type);
        Debug.Assert(converter is IGenericJsonConverter);

        return ((IGenericJsonConverter)converter).ReadGeneric<T>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var type = value.GetType();
        if (type == _model.Type)
        {
            if (_inner is null)
            {
                ThrowHelper.ThrowInvalidOperationException($"Type {type} cannot be serialized itself, a configured subtype must be used instead.");
            }

            // Not a subtype of the polymorphic model, so use the default converter
            _inner.Write(writer, value, options);
            return;
        }

        if (!_model.TryGetDescendantModel(value.GetType(), out var model))
        {
            ThrowHelper.ThrowInvalidOperationException($"No model found for type '{value.GetType()}' in context of polymorphic model '{_model.Type}'.");
        }

        var converter = options.GetConverter(model.Type);
        Debug.Assert(converter is IGenericJsonConverter);

        ((IGenericJsonConverter)converter).WriteGeneric(writer, value, options);
    }

    private FieldValue<NonExhaustiveEnum<TDiscriminator>> FindDiscriminator(
        in Utf8JsonReader originalReader,
        Span<char> propertyScratch,
        JsonSerializerOptions options)
    {
        var reader = originalReader;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected property name but got '{reader.TokenType}'");
            }

            var length = reader.HasValueSequence
                ? reader.ValueSequence.Length
                : reader.ValueSpan.Length;

            if (length > propertyScratch.Length)
            {
                // Skip unknown property
                reader.Skip();
                continue;
            }

            var propLength = reader.CopyString(propertyScratch);
            var propName = propertyScratch[..propLength];

            if (!reader.Read())
            {
                throw new JsonException($"Expected property value but got '{reader.TokenType}'");
            }

            if (!_propertyNameComparer.Equals(propName, _discriminatorPropertyModel.Name))
            {
                // Skip non-discriminator property
                reader.Skip();
                continue;
            }

            return _discriminatorPropertyModel.Read(ref reader, options);
        }

        return default;
    }

    /// <inheritdoc/>
    void IGenericJsonConverter.WriteGeneric<T1>(Utf8JsonWriter writer, T1 value, JsonSerializerOptions options)
        where T1 : class
    {
        Debug.Assert(value.GetType().IsAssignableTo(typeof(T)));

        Write(writer, (T)(object)value, options);
    }

    /// <inheritdoc/>
    T1? IGenericJsonConverter.ReadGeneric<T1>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        where T1 : class
    {
        Debug.Assert(typeof(T).IsAssignableTo(typeof(T1)));

        return (T1?)(object?)Read(ref reader, typeof(T1), options);
    }
}
