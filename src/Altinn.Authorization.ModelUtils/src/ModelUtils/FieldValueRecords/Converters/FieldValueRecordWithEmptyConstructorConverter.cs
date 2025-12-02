using Altinn.Authorization.ModelUtils.FieldValueRecords.Json;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

/// <summary>
/// A <see cref="FieldValueRecordBaseConverter{T}"/> for field-value-records with an empty constructor.
/// </summary>
/// <typeparam name="T">The field-value-record type.</typeparam>
internal class FieldValueRecordWithEmptyConstructorConverter<T>
    : FieldValueRecordBaseConverter<T>
    where T : class
{
    private readonly FrozenDictionary<PropertyName, Property> _propLookup;
    private readonly ImmutableArray<Property> _properties;
    private readonly int _propertyMaxLength;

    internal FieldValueRecordWithEmptyConstructorConverter(
        IFieldValueRecordModel<T> model,
        JsonSerializerOptions options)
        : base(model)
    {
        Debug.Assert(model.Constructor is not null);
        Debug.Assert(model.Constructor.Parameters.Length == 0);

        var comparer = GetPropertyComparer(options);
        var modelProperties = model.Properties(includeInherited: true);
        var properties = new Dictionary<PropertyName, Property>(modelProperties.Length, comparer);
        var propertiesList = new List<Property>(modelProperties.Length);

        foreach (var propModel in FieldValueRecordPropertyJsonModel<T>.Create(modelProperties, options))
        {
            var prop = new Property(propModel);

            // earlier properties are from more concrete types and takes precedence
            if (properties.TryAdd(prop.Name, prop))
            {
                propertiesList.Add(prop);
            }
        }

        _propLookup = properties.ToFrozenDictionary(comparer);
        _properties = [.. propertiesList];
        _propertyMaxLength = _properties.Max(static p => p.Name.Name.Length);

        var index = 0;
        foreach (var prop in _properties)
        {
            prop.PropertyIndex = index++;
        }
    }

    protected override ImmutableArray<Property> Properties => _properties;

    protected override FrozenDictionary<PropertyName, Property> PropertyLookup => _propLookup;

    protected internal override int PropertyMaxLength => _propertyMaxLength;

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

        Debug.Assert(Model.Constructor is not null);
        T result = Model.Constructor.Invoke([]);

        PopulateObject(result, ref reader, options);
        return result;
    }
}
