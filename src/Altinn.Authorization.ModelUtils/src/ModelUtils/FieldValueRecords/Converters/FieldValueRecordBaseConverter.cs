using Altinn.Authorization.ModelUtils.FieldValueRecords.Json;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

/// <summary>
/// Base class for <see cref="JsonConverter"/>s for field-value-records.
/// </summary>
/// <typeparam name="T">The field-value-record type.</typeparam>
internal abstract class FieldValueRecordBaseConverter<T>
    : JsonConverter<T>
    , IFieldValueRecordJsonConverter
    , IGenericJsonConverter
    where T : class
{
    /// <summary>
    /// Gets the <see cref="FieldValueRecordModel{T}"/>.
    /// </summary>
    protected IFieldValueRecordModel<T> Model { get; }

    protected FieldValueRecordBaseConverter(
        IFieldValueRecordModel<T> model)
    {
        Model = model;
    }

    /// <summary>
    /// Gets the property lookup dictionary.
    /// </summary>
    protected abstract FrozenDictionary<PropertyName, Property> PropertyLookup { get; }

    /// <summary>
    /// Gets the list of properties, in write order.
    /// </summary>
    protected abstract ImmutableArray<Property> Properties { get; }

    /// <summary>
    /// Gets the maximum length of the property names.
    /// </summary>
    protected internal abstract int PropertyMaxLength { get; }

    /// <inheritdoc/>
    IFieldValueRecordModel IFieldValueRecordJsonConverter.Model
        => Model;

    /// <inheritdoc/>
    bool IFieldValueRecordJsonConverter.TryFindPropertyModel(string name, [NotNullWhen(true)] out IFieldValueRecordPropertyModel? model)
    {
        if (PropertyLookup.GetAlternateLookup<string>().TryGetValue(name, out var prop))
        {
            model = prop.Model;
            return true;
        }

        model = null;
        return false;
    }

    /// <inheritdoc/>
    public override sealed void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var prop in Properties)
        {
            if (!prop.CanRead)
            {
                continue;
            }

            prop.WriteFrom(writer, value, options);
        }
        writer.WriteEndObject();
    }
    
    /// <summary>
    /// Populates an object from JSON data using a reader and specified options, ensuring required properties are set.
    /// </summary>
    /// <param name="instance">The object to be populated with data from the JSON reader.</param>
    /// <param name="reader">Reads JSON data to extract property values for the object.</param>
    /// <param name="options">Specifies serialization options that affect how the JSON is processed.</param>
    /// <exception cref="JsonException">Thrown when a required property is missing or an unexpected token is encountered.</exception>
    protected void PopulateObject(T instance, ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        char[]? propertyScratchArray = null;
        bool[]? propertiesSetArray = null;

        try
        {
            propertyScratchArray = ArrayPool<char>.Shared.Rent(PropertyMaxLength);
            propertiesSetArray = ArrayPool<bool>.Shared.Rent(Properties.Length);

            var propertyScratch = propertyScratchArray.AsSpan(0, PropertyMaxLength);
            var propertiesSet = propertiesSetArray.AsSpan(0, Properties.Length);
            propertiesSet.Clear();

            var lookup = PropertyLookup.GetAlternateLookup<ReadOnlySpan<char>>();

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
                    // Unexpected end of JSON data
                    throw new JsonException("Unexpected end of JSON data while reading property value.");
                }

                if (!lookup.TryGetValue(propName, out var prop))
                {
                    // Skip unknown property
                    reader.Skip();
                    continue;
                }

                if (prop.IsConstructorParameter || !prop.CanWrite)
                {
                    // Skip constructor parameters and non-writable properties
                    reader.Skip();
                    continue;
                }

                prop.ReadInto(instance, ref reader, options);
                propertiesSet[prop.PropertyIndex] = true;
            }

            List<PropertyName>? missingProperties = null;
            foreach (var prop in Properties)
            {
                if (!prop.IsConstructorParameter && prop.CanWrite && !propertiesSet[prop.PropertyIndex] && prop.IsRequired)
                {
                    missingProperties ??= [];
                    missingProperties.Add(prop.Name);
                }
            }

            if (missingProperties is { Count: > 0 })
            {
                var message = $"Missing required properties: {string.Join(", ", missingProperties)}";
                throw new JsonException(message);
            }
        }
        finally
        {
            if (propertyScratchArray is not null)
            {
                propertyScratchArray.AsSpan().Clear();
                ArrayPool<char>.Shared.Return(propertyScratchArray);
            }

            if (propertiesSetArray is not null)
            {
                ArrayPool<bool>.Shared.Return(propertiesSetArray);
            }
        }
    }

    /// <summary>
    /// Retrieves a comparer for property names based on json serialization settings.
    /// </summary>
    /// <param name="options">Specifies the settings that determine how property names are compared.</param>
    /// <returns>Returns a comparer that is either case-sensitive or case-insensitive.</returns>
    protected internal static PropertyName.Comparer GetPropertyComparer(JsonSerializerOptions options)
        => options.PropertyNameCaseInsensitive
            ? PropertyName.Comparer.OrdinalIgnoreCase
            : PropertyName.Comparer.Ordinal;

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

    protected class Property
    {
        private readonly IFieldValueRecordPropertyJsonModel<T> _model;

        public Property(IFieldValueRecordPropertyJsonModel<T> model)
        {
            _model = model;
        }

        public IFieldValueRecordPropertyModel Model => _model.Model;

        public PropertyName Name => _model.Name;

        public Type Type => _model.Type;

        public bool CanWrite => _model.CanWrite;

        public bool CanRead => _model.CanRead;

        public bool IsRequired => _model.IsRequired;

        public int PropertyIndex { get; set; } = -1;

        public int ParameterIndex { get; set; } = -1;

        public FieldValue<object> DefaultParameterValue { get; set; }

        public bool IsConstructorParameter => ParameterIndex != -1;

        public void WriteFrom(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => _model.WriteFrom(value, Name.Encoded, writer, options);

        public void ReadInto(T value, ref Utf8JsonReader reader, JsonSerializerOptions options)
            => _model.ReadInto(value, ref reader, options);

        public void ReadConstructorParameter(ref object? slot, ref Utf8JsonReader reader, JsonSerializerOptions options)
            => _model.ReadConstructorParameter(ref slot, ref reader, options);
    }
}
