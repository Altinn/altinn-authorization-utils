using Altinn.Authorization.ModelUtils.FieldValueRecords.Json;
using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

/// <summary>
/// A <see cref="FieldValueRecordBaseConverter{T}"/> for field-value-records with a parameterized constructor.
/// </summary>
/// <typeparam name="T">The field-value-record type.</typeparam>
internal class FieldValueRecordWithParameterizedConstructorConverter<T>
    : FieldValueRecordBaseConverter<T>
    where T : class
{
    const byte PARAM_STATE_UNSET = 0;
    const byte PARAM_STATE_DEFAULT = 1;
    const byte PARAM_STATE_SET = 2;

    private readonly FrozenDictionary<PropertyName, Property> _propLookup;
    private readonly ImmutableArray<Property> _properties;
    private readonly int _propertyMaxLength;

    internal FieldValueRecordWithParameterizedConstructorConverter(
        IFieldValueRecordModel<T> model,
        JsonSerializerOptions options)
        : base(model)
    {
        Debug.Assert(model.Constructor is not null);
        Debug.Assert(model.Constructor.Parameters.Length > 0);

        var comparer = GetPropertyComparer(options);
        var modelProperties = model.Properties(includeInherited: true);
        var properties = new Dictionary<PropertyName, Property>(modelProperties.Length, comparer);
        var propertiesList = new List<Property>(modelProperties.Length);
        var parametersList = new List<Property>(model.Constructor.Parameters.Length);

        foreach (var propModel in FieldValueRecordPropertyJsonModel<T>.Create(modelProperties, options))
        {
            var prop = new Property(propModel);

            // earlier properties are from more concrete types and takes precedence
            if (properties.TryAdd(prop.Name, prop))
            {
                propertiesList.Add(prop);
            }
        }

        var index = 0;
        var propertiesByCaseInsensitiveString = properties.ToDictionary(
            static kvp => kvp.Key.Name,
            static kvp => kvp.Value,
            StringComparer.OrdinalIgnoreCase);

        foreach (var arg in model.Constructor.Parameters)
        {
            if (string.IsNullOrEmpty(arg.Name))
            {
                ThrowHelper.ThrowInvalidOperationException("Constructor parameter name is null or empty");
            }

            // We're using case-insensitive string comparison here, because typically constructor-parameter names are not the same case as the property names.
            var propName = PropertyName.ConvertName(arg.Name, options.PropertyNamingPolicy);
            if (!propertiesByCaseInsensitiveString.TryGetValue(propName, out var prop))
            {
                ThrowHelper.ThrowInvalidOperationException($"Constructor parameter '{arg.Name}' does not match any property");
            }

            if (prop.Type != arg.Type)
            {
                ThrowHelper.ThrowInvalidOperationException($"Constructor parameter '{arg.Name}' does not match property type");
            }

            prop.ParameterIndex = index++;
            prop.DefaultParameterValue = arg.DefaultValue;
            parametersList.Add(prop);
            propertiesList.Remove(prop);
        }

        _propLookup = properties.ToFrozenDictionary(comparer);
        _properties = [.. parametersList, .. propertiesList];
        _propertyMaxLength = _properties.Max(static p => p.Name.Name.Length);

        index = 0;
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

        T result;

        Debug.Assert(Model.Constructor is not null);
        var parameterLength = Model.Constructor.Parameters.Length;
        object?[]? parametersScratch = null;
        byte[]? parametersSetScratch = null;
        char[]? propertyScratch = null;

        try
        {
            parametersScratch = ArrayPool<object?>.Shared.Rent(parameterLength);
            parametersSetScratch = ArrayPool<byte>.Shared.Rent(parameterLength);
            propertyScratch = ArrayPool<char>.Shared.Rent(_propertyMaxLength);

            parametersSetScratch.AsSpan().Clear();
            result = CreateObject(
                in reader,
                parametersScratch.AsSpan(0, parameterLength),
                parametersSetScratch.AsSpan(0, parameterLength),
                propertyScratch.AsSpan(0, _propertyMaxLength),
                options);
        }
        finally
        {
            if (parametersScratch is not null)
            {
                parametersScratch.AsSpan().Clear();
                ArrayPool<object?>.Shared.Return(parametersScratch);
            }

            if (parametersSetScratch is not null)
            {
                ArrayPool<byte>.Shared.Return(parametersSetScratch);
            }

            if (propertyScratch is not null)
            {
                propertyScratch.AsSpan().Clear();
                ArrayPool<char>.Shared.Return(propertyScratch);
            }
        }

        PopulateObject(result, ref reader, options);
        return result;
    }

    protected T CreateObject(
        in Utf8JsonReader originalReader,
        Span<object?> parameterSlots,
        Span<byte> parametersSet,
        Span<char> propertyScratch,
        JsonSerializerOptions options)
    {
        foreach (var ctorParam in _properties.Where(static p => p.IsConstructorParameter && p.DefaultParameterValue.IsSet))
        {
            parameterSlots[ctorParam.ParameterIndex] = ctorParam.DefaultParameterValue.Value;
            parametersSet[ctorParam.ParameterIndex] = PARAM_STATE_DEFAULT;
        }

        var reader = originalReader;
        var lookup = _propLookup.GetAlternateLookup<ReadOnlySpan<char>>();

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
                reader.SafeSkip();
                continue;
            }

            var propLength = reader.CopyString(propertyScratch);
            var propName = propertyScratch[..propLength];

            if (!reader.Read())
            {
                throw new JsonException("Unexpected end of JSON while reading property value");
            }

            if (!lookup.TryGetValue(propName, out var prop))
            {
                // Skip unknown property
                reader.SafeSkip();
                continue;
            }

            if (!prop.IsConstructorParameter)
            {
                // Skip normal properties
                reader.SafeSkip();
                continue;
            }

            var index = prop.ParameterIndex;
            prop.ReadConstructorParameter(ref parameterSlots[index], ref reader, options);
            parametersSet[index] = PARAM_STATE_SET;

            if (!parametersSet.ContainsAnyExcept(PARAM_STATE_SET))
            {
                // all parameters found
                break;
            }
        }

        var missingPropertyIndex = parametersSet.IndexOf(PARAM_STATE_UNSET);
        if (missingPropertyIndex > -1)
        {
            var missingProperty = _properties.First(p => p.ParameterIndex == missingPropertyIndex);
            throw new JsonException($"Missing required constructor parameter '{missingProperty.Name.Name}'");
        }

        Debug.Assert(Model.Constructor is not null);
        return Model.Constructor.Invoke(parameterSlots);
    }
}
