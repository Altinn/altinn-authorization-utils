using CommunityToolkit.Diagnostics;
using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

////internal class FieldValueRecordConverter<T>
////    : JsonConverter<T>
////    where T : class
////{
////    public FieldValueRecordConverter<T> Create(
////        FieldValueRecordModel<T> model,
////        JsonSerializerOptions options)
////    {
////        if (model.Constructor.Parameters.IsEmpty)
////        {
////            return new(model, options);
////        }

////        if (model.Constructor.Parameters.Length <= 4)
////        {
////            return new RecordConverterWithFewConstructorParameters(model, options);
////        }

////        return new RecordConverterWithConstructorParameters(model, options);
////    }

////    protected FieldValueRecordConverter(
////        FieldValueRecordModel<T> model,
////        JsonSerializerOptions options)
////    {
////    }

////    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
////    {
////        throw new NotImplementedException();
////    }

////    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
////    {
////        throw new NotImplementedException();
////    }

////    protected virtual T Create(in Utf8JsonReader reader, JsonSerializerOptions options)
////    {
////        throw new NotImplementedException();
////    }

////    protected virtual void ReadConstructorParameters(
////        in Utf8JsonReader reader,
////        Span<object?> slots,
////        JsonSerializerOptions options)
////    {

////    }

////    protected virtual T Create(in Utf8JsonReader reader, Span<object?> parameterSlots, JsonSerializerOptions options)
////    {
////        throw new NotImplementedException();
////    }

////    private sealed class RecordConverterWithFewConstructorParameters
////        : FieldValueRecordConverter<T>
////    {
////        private readonly int _parameterCount;

////        public RecordConverterWithFewConstructorParameters(
////            FieldValueRecordModel<T> model,
////            JsonSerializerOptions options)
////            : base(model, options)
////        {
////            Debug.Assert(model.Constructor.Parameters.Length <= 4);
////            Debug.Assert(model.Constructor.Parameters.Length > 0);

////            _parameterCount = model.Constructor.Parameters.Length;
////        }

////        protected override T Create(in Utf8JsonReader reader, JsonSerializerOptions options)
////        {
////            Span<object?> parameters = [null, null, null, null];

////            return Create(in reader, parameters[.._parameterCount], options);
////        }
////    }

////    private sealed class RecordConverterWithConstructorParameters
////        : FieldValueRecordConverter<T>
////    {
////        private readonly int _parameterCount;

////        public RecordConverterWithConstructorParameters(
////            FieldValueRecordModel<T> model,
////            JsonSerializerOptions options)
////            : base(model, options)
////        {
////            Debug.Assert(model.Constructor.Parameters.Length > 4);

////            _parameterCount = model.Constructor.Parameters.Length;
////        }

////        protected override T Create(in Utf8JsonReader reader, JsonSerializerOptions options)
////        {
////            object?[] parameters = ArrayPool<object?>.Shared.Rent(_parameterCount);

////            try
////            {
////                return Create(in reader, parameters.AsSpan(0, _parameterCount), options);
////            }
////            finally
////            {
////                ArrayPool<object?>.Shared.Return(parameters);
////            }
////        }
////    }
////}

internal interface IFieldValueRecordConstructorParameterJsonModel<in TOwner>
    where TOwner : class
{
    public object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
}

internal static class FieldValueRecordConstructorJsonModel<TOwner>
    where TOwner : class
{
}

internal sealed class FieldValueRecordConstructorJsonModel<TOwner, TValue>
    : IFieldValueRecordConstructorParameterJsonModel<TOwner>
    where TOwner : class
    where TValue : notnull
{
    private readonly IFieldValueRecordConstructorParameterModel<TOwner, TValue> _property;

    public FieldValueRecordConstructorJsonModel(IFieldValueRecordConstructorParameterModel<TOwner, TValue> property)
    {
        _property = property;
    }

    public object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return FieldValue.Null;
        }

        return JsonSerializer.Deserialize<TValue>(ref reader, options);
    }
}

internal interface IFieldValueRecordPropertyJsonModel<in TOwner>
    where TOwner : class
{
    public PropertyName Name { get; }

    public Type Type { get; }

    public bool CanRead { get; }

    public bool CanWrite { get; }

    public bool IsRequired { get; }

    // used for constructor invocations
    public object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options);

    public void ReadInto(TOwner owner, ref Utf8JsonReader reader, JsonSerializerOptions options);

    public void WriteFrom(TOwner owner, JsonEncodedText propertyName, Utf8JsonWriter writer, JsonSerializerOptions options);
}

internal static class FieldValueRecordPropertyJsonModel<TOwner>
    where TOwner : class
{
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

    public PropertyName Name => _name;

    public Type Type => _inner.Type;

    public bool CanRead => _inner.CanRead;

    public bool CanWrite => _inner.CanWrite;

    public bool IsRequired => _inner.IsRequired;

    public object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<TValue>(ref reader, options);

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

internal abstract class FieldValueRecordBaseConverter<T>
    : JsonConverter<T>
    where T : class
{
    protected FieldValueRecordModel<T> Model { get; }

    protected FieldValueRecordBaseConverter(
        FieldValueRecordModel<T> model)
    {
        Model = model;
    }

    protected abstract FrozenDictionary<PropertyName, Property> PropertyLookup { get; }

    protected abstract ImmutableArray<Property> Properties { get; }
    
    protected abstract int PropertyMaxLength { get; }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
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

    protected static PropertyNameComparer GetPropertyComparer(JsonSerializerOptions options)
        => options.PropertyNameCaseInsensitive
            ? PropertyNameComparer.OrdinalIgnoreCase
            : PropertyNameComparer.Ordinal;

    protected class Property
    {
        private readonly IFieldValueRecordPropertyJsonModel<T> _model;

        public Property(IFieldValueRecordPropertyJsonModel<T> model)
        {
            _model = model;
        }

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

        public object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
            => _model.Read(ref reader, options);
    }
}

internal class FieldValueRecordConverter<T>
    : FieldValueRecordBaseConverter<T>
    where T : class
{
    private readonly FrozenDictionary<PropertyName, Property> _propLookup;
    private readonly ImmutableArray<Property> _properties;
    private readonly int _propertyMaxLength;

    internal FieldValueRecordConverter(
        FieldValueRecordModel<T> model,
        JsonSerializerOptions options)
        : base(model)
    {
        Debug.Assert(model.Constructor.Parameters.Length == 0);

        var comparer = GetPropertyComparer(options);
        var modelProperties = model.Properties(includeInherited: true);
        var properties = new Dictionary<PropertyName, Property>(modelProperties.Length, comparer);
        var propertiesList = new List<Property>(modelProperties.Length);

        foreach (var propModel in FieldValueRecordPropertyJsonModel<T>.Create(modelProperties.Cast<IFieldValueRecordPropertyModel<T>>(), options))
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

    protected override int PropertyMaxLength => _propertyMaxLength;

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

        T result = Model.Constructor.Invoke([]);

        PopulateObject(result, ref reader, options);
        return result;
    }
}

internal class FieldValueRecordWithParameterizedConstructorConverter<T>
    : FieldValueRecordBaseConverter<T>
    where T : class
{
    private readonly FrozenDictionary<PropertyName, Property> _propLookup;
    private readonly ImmutableArray<Property> _properties;
    private readonly int _propertyMaxLength;

    internal FieldValueRecordWithParameterizedConstructorConverter(
        FieldValueRecordModel<T> model,
        JsonSerializerOptions options)
        : base(model)
    {
        Debug.Assert(model.Constructor.Parameters.Length > 0);

        var comparer = GetPropertyComparer(options);
        var modelProperties = model.Properties(includeInherited: true);
        var properties = new Dictionary<PropertyName, Property>(modelProperties.Length, comparer);
        var propertiesByString = properties.GetAlternateLookup<string>();
        var propertiesList = new List<Property>(modelProperties.Length);
        var parametersList = new List<Property>(model.Constructor.Parameters.Length);

        foreach (var propModel in FieldValueRecordPropertyJsonModel<T>.Create(modelProperties.Cast<IFieldValueRecordPropertyModel<T>>(), options))
        {
            var prop = new Property(propModel);

            // earlier properties are from more concrete types and takes precedence
            if (properties.TryAdd(prop.Name, prop))
            {
                propertiesList.Add(prop);
            }
        }

        var index = 0;
        foreach (var arg in model.Constructor.Parameters)
        {
            if (string.IsNullOrEmpty(arg.Name))
            {
                ThrowHelper.ThrowInvalidOperationException("Constructor parameter name is null or empty");
            }

            var propName = PropertyName.ConvertName(arg.Name, options.PropertyNamingPolicy);
            if (!propertiesByString.TryGetValue(propName, out var prop))
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

    protected override int PropertyMaxLength => _propertyMaxLength;

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

        var parameterLength = Model.Constructor.Parameters.Length;
        object?[]? parametersScratch = null;
        bool[]? parametersSetScratch = null;
        char[]? propertyScratch = null;

        try
        {
            parametersScratch = ArrayPool<object?>.Shared.Rent(parameterLength);
            parametersSetScratch = ArrayPool<bool>.Shared.Rent(parameterLength);
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
                ArrayPool<bool>.Shared.Return(parametersSetScratch);
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
        Span<bool> parametersSet,
        Span<char> propertyScratch,
        JsonSerializerOptions options)
    {
        foreach (var ctorParam in _properties.Where(static p => p.IsConstructorParameter && p.DefaultParameterValue.IsSet))
        {
            parameterSlots[ctorParam.ParameterIndex] = ctorParam.DefaultParameterValue.Value;
            parametersSet[ctorParam.ParameterIndex] = true;
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
                reader.Skip();
                continue;
            }

            var propLength = reader.CopyString(propertyScratch);
            var propName = propertyScratch[..propLength];

            if (!lookup.TryGetValue(propName, out var prop))
            {
                // Skip unknown property
                reader.Skip();
                continue;
            }

            if (!prop.IsConstructorParameter)
            {
                // Skip normal properties
                reader.Skip();
                continue;
            }

            var index = prop.ParameterIndex;
            parameterSlots[index] = prop.Read(ref reader, options);
            parametersSet[index] = true;

            if (!parametersSet.Contains(false))
            {
                // all parameters found
                break;
            }
        }

        var missingPropertyIndex = parametersSet.IndexOf(false);
        if (missingPropertyIndex > -1)
        {
            var missingProperty = _properties.First(p => p.ParameterIndex == missingPropertyIndex);
            throw new JsonException($"Missing required constructor parameter '{missingProperty.Name.Name}'");
        }

        return Model.Constructor.Invoke(parameterSlots);
    }
}

[DebuggerDisplay("{_name}")]
internal class PropertyName
{
    private readonly string _name;
    private readonly JsonEncodedText _encoded;

    internal static string ConvertName(string name, JsonNamingPolicy? propertyNamingPolicy)
    {
        Guard.IsNotNullOrWhiteSpace(name);

        if (propertyNamingPolicy is not null)
        {
            name = propertyNamingPolicy.ConvertName(name);
        }

        return name;
    }

    internal static PropertyName Create(string name, JsonNamingPolicy? propertyNamingPolicy)
    {
        name = ConvertName(name, propertyNamingPolicy);

        var encoded = JsonEncodedText.Encode(name);
        return new PropertyName(name, encoded);
    }

    private PropertyName(string name, JsonEncodedText encoded)
    {
        _name = name;
        _encoded = encoded;
    }

    public string Name => _name;

    public JsonEncodedText Encoded => _encoded;

    public override string ToString()
        => _name;
}

internal class PropertyNameComparer
    : IEqualityComparer<PropertyName>
    , IAlternateEqualityComparer<string, PropertyName?>
    , IAlternateEqualityComparer<ReadOnlySpan<char>, PropertyName?>
{
    public static readonly PropertyNameComparer Ordinal = new(StringComparer.Ordinal);
    public static readonly PropertyNameComparer OrdinalIgnoreCase = new(StringComparer.OrdinalIgnoreCase);

    private readonly IEqualityComparer<string> _inner;
    private readonly IAlternateEqualityComparer<ReadOnlySpan<char>, string?> _innerSpan;

    private PropertyNameComparer(IEqualityComparer<string> inner)
    {
        Guard.IsAssignableToType<IAlternateEqualityComparer<ReadOnlySpan<char>, string?>>(inner);

        _inner = inner;
        _innerSpan = (IAlternateEqualityComparer<ReadOnlySpan<char>, string?>)_inner;
    }

    PropertyName? IAlternateEqualityComparer<string, PropertyName?>.Create(string alternate)
    {
        throw new NotSupportedException("It's not possible to create PropertyNames using the PropertyNameComparer");
    }

    PropertyName? IAlternateEqualityComparer<ReadOnlySpan<char>, PropertyName?>.Create(ReadOnlySpan<char> alternate)
    {
        throw new NotSupportedException("It's not possible to create PropertyNames using the PropertyNameComparer");
    }

    public bool Equals(PropertyName? x, PropertyName? y)
        => _inner.Equals(x?.Name, y?.Name);

    public bool Equals(string alternate, PropertyName? other)
        => _inner.Equals(alternate, other?.Name);

    public bool Equals(ReadOnlySpan<char> alternate, PropertyName? other)
        => _innerSpan.Equals(alternate, other?.Name);

    public int GetHashCode([DisallowNull] PropertyName obj)
        => _inner.GetHashCode(obj.Name);

    public int GetHashCode(string alternate)
        => _inner.GetHashCode(alternate);

    public int GetHashCode(ReadOnlySpan<char> alternate)
        => _innerSpan.GetHashCode(alternate);
}

//internal class FieldValueRecordWithSmallParameterizedConstructorConverter<T>
//    : FieldValueRecordWithParameterizedConstructorConverter<T>
//    where T : class
//{
//}
