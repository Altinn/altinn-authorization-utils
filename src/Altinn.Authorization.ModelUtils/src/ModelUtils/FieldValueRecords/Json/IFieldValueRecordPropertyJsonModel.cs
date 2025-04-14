using Altinn.Authorization.ModelUtils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Json;

/// <summary>
/// Defines properties and methods for handling JSON serialization and deserialization of field values.
/// </summary>
/// <typeparam name="TOwner">Represents the type of the object that owns the field value being processed.</typeparam>
internal interface IFieldValueRecordPropertyJsonModel<in TOwner>
    where TOwner : class
{
    /// <summary>
    /// Gets the underlying model.
    /// </summary>
    public IFieldValueRecordPropertyModel<TOwner> Model { get; }

    /// <summary>
    /// Gets the json name of the property.
    /// </summary>
    public PropertyName Name { get; }

    /// <summary>
    /// Gets the type of the property.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets whether the property can be read.
    /// </summary>
    public bool CanRead { get; }

    /// <summary>
    /// Gets whether the property can be written to.
    /// </summary>
    public bool CanWrite { get; }

    /// <summary>
    /// Gets whether the property is mandatory.
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// Reads a constructor parameter from a JSON source and populates the provided slot with the value.
    /// </summary>
    /// <param name="slot">Slot to populate with the value read from <paramref name="reader"/>.</param>
    /// <param name="reader">Provides the JSON data to be read and parsed for the constructor parameter.</param>
    /// <param name="options">Contains settings that influence the JSON serialization and deserialization process.</param>
    public void ReadConstructorParameter(ref object? slot, ref Utf8JsonReader reader, JsonSerializerOptions options);

    /// <summary>
    /// Reads JSON data into a specified object, populating the property based on the provided reader.
    /// </summary>
    /// <param name="owner">The object that will be populated with data from the JSON reader.</param>
    /// <param name="reader">The JSON reader that provides the data to be read into the object.</param>
    /// <param name="options">Options that control the behavior of the JSON serialization process.</param>
    public void ReadInto(TOwner owner, ref Utf8JsonReader reader, JsonSerializerOptions options);

    /// <summary>
    /// Writes JSON data from a specified owner to a writer using provided options.
    /// </summary>
    /// <param name="owner">Represents the source object from which data is being written.</param>
    /// <param name="propertyName">Indicates the name of the property being serialized.</param>
    /// <param name="writer">The writer that outputs the JSON data.</param>
    /// <param name="options">Contains settings that control the serialization process.</param>
    public void WriteFrom(TOwner owner, JsonEncodedText propertyName, Utf8JsonWriter writer, JsonSerializerOptions options);
}

/// <summary>
/// Helper methods to create <see cref="IFieldValueRecordPropertyJsonModel{TOwner}"/>s.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
internal static class FieldValueRecordPropertyJsonModel<TOwner>
    where TOwner : class
{
    /// <summary>
    /// Creates <see cref="IFieldValueRecordPropertyJsonModel{TOwner}"/>s from the given
    /// enumerable of <see cref="IFieldValueRecordPropertyModel{TOwner}"/>s and the given
    /// <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <param name="options">The json serializer options.</param>
    /// <returns>An enumerable of <see cref="IFieldValueRecordPropertyJsonModel{TOwner}"/>s.</returns>
    public static IEnumerable<IFieldValueRecordPropertyJsonModel<TOwner>> Create(
        IEnumerable<IFieldValueRecordPropertyModel<TOwner>> properties,
        JsonSerializerOptions options)
    {
        var visitor = new FactoryVisitor(options);
        return properties.Select(p => p.Accept(visitor));
    }

    private sealed class FactoryVisitor(JsonSerializerOptions options)
        : IFieldValueRecordPropertyModelVisitor<TOwner, IFieldValueRecordPropertyJsonModel<TOwner>>
    {
        public IFieldValueRecordPropertyJsonModel<TOwner> Visit<TValue>(IFieldValueRecordPropertyModel<TOwner, TValue> property)
            where TValue : notnull
            => new FieldValueRecordPropertyJsonModel<TOwner, TValue>(property, options);
    }
}

/// <summary>
/// Implementation of <see cref="IFieldValueRecordPropertyJsonModel{TOwner}"/>.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The property type.</typeparam>
internal sealed class FieldValueRecordPropertyJsonModel<TOwner, TValue>
    : IFieldValueRecordPropertyJsonModel<TOwner>
    where TOwner : class
    where TValue : notnull
{
    private readonly JsonIgnoreCondition _ignoreCondition;
    private readonly IFieldValueRecordPropertyModel<TOwner, TValue> _inner;
    private readonly PropertyName _name;

    public FieldValueRecordPropertyJsonModel(
        IFieldValueRecordPropertyModel<TOwner, TValue> inner,
        JsonSerializerOptions options)
    {
        _inner = inner;
        _ignoreCondition = inner.GetCustomAttribute<JsonIgnoreAttribute>(inherit: true)?.Condition ?? options.DefaultIgnoreCondition;
        _name = PropertyName.Create(inner.Name, options.PropertyNamingPolicy);
    }

    /// <inheritdoc/>
    public IFieldValueRecordPropertyModel<TOwner> Model => _inner;

    /// <inheritdoc/>
    public PropertyName Name => _name;

    /// <inheritdoc/>
    public Type Type => _inner.Type;

    /// <inheritdoc/>
    public bool CanRead => _inner.CanRead;

    /// <inheritdoc/>
    public bool CanWrite => _inner.CanWrite;

    /// <inheritdoc/>
    public bool IsRequired => _inner.IsRequired;

    /// <inheritdoc/>
    public void ReadConstructorParameter(ref object? slot, ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            _inner.WriteSlot(ref slot, FieldValue.Null);
            return;
        }

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        if (value is null)
        {
            _inner.WriteSlot(ref slot, FieldValue.Null);
            return;
        }

        _inner.WriteSlot(ref slot, value);
    }

    /// <inheritdoc/>
    public void ReadInto(TOwner owner, ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (_ignoreCondition == JsonIgnoreCondition.Always)
        {
            return;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            _inner.Write(owner, FieldValue.Null);
            return;
        }

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        if (value is null)
        {
            _inner.Write(owner, FieldValue.Null);
            return;
        }

        _inner.Write(owner, value);
    }

    /// <inheritdoc/>
    public void WriteFrom(TOwner owner, JsonEncodedText propertyName, Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        var value = _inner.Read(owner);

        if (_ignoreCondition == JsonIgnoreCondition.Always || value.IsUnset)
        {
            return;
        }

        if (value.IsNull)
        {
            if (_ignoreCondition == JsonIgnoreCondition.WhenWritingNull)
            {
                return;
            }

            writer.WriteNull(propertyName);
            return;
        }

        writer.WritePropertyName(propertyName);
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
