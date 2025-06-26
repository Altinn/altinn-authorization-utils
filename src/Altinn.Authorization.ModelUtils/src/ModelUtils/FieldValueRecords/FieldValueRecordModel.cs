using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// Field-value-record model factory.
/// </summary>
public static class FieldValueRecordModel
{
    private static readonly ConcurrentDictionary<Type, IFieldValueRecordModel> _cache = new();

    /// <summary>
    /// Gets the model for the specified type.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>A <see cref="FieldValueRecordModel{T}"/> for <typeparamref name="T"/>.</returns>
    public static IFieldValueRecordModel<T> For<T>()
        where T : class
        => (IFieldValueRecordModel<T>)For(typeof(T));

    /// <summary>
    /// Gets the model for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A <see cref="FieldValueRecordModel"/> for <paramref name="type"/>.</returns>
    public static IFieldValueRecordModel For(Type type)
        => _cache.GetOrAdd(type, static t => CreateModel(t));

    private static IFieldValueRecordModel CreateModel(Type type)
    {
        var modelType = typeof(FieldValueRecordModel<>).MakeGenericType(type);
        var instanceProperty = modelType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        Debug.Assert(instanceProperty is not null);

        var model = instanceProperty.GetValue(null);
        Debug.Assert(model is IFieldValueRecordModel);

        return (IFieldValueRecordModel)model;
    }
}

/// <summary>
/// A model for a record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
/// <typeparam name="T">The type of the record.</typeparam>
internal sealed class FieldValueRecordModel<T>
    : IFieldValueRecordModel<T>
    where T : class
{
    /// <summary>
    /// Gets the singleton instance of the model.
    /// </summary>
    public static FieldValueRecordModel<T> Instance { get; } = new();

    private readonly IFieldValueRecordModel? _parent;
    private readonly IFieldValueRecordConstructorModel<T>? _constructor;
    private readonly ImmutableArray<IFieldValueRecordPropertyModel<T>> _declaredProperties;
    private readonly ImmutableArray<IFieldValueRecordPropertyModel<T>> _allProperties;
    private readonly IFieldValueRecordPropertyModel<T, JsonElement>? _extensionDataProperty;

    private FieldValueRecordModel()
    {
        if (typeof(T).BaseType is { } baseType && baseType != typeof(object))
        {
            _parent = FieldValueRecordModel.For(baseType);
        }

        var ctor = FindConstructor();
        var declaredProperties = GetDeclaredProperties().ToImmutableArray();
        var inheritedProperties = _parent is null ? [] : _parent.Properties(includeInherited: true).CastArray<IFieldValueRecordPropertyModel<T>>();
        var extensionDataProperty = GetExtensionDataProperty(ref declaredProperties, _parent);
        
        _constructor = ctor;
        _declaredProperties = declaredProperties;
        _allProperties = [.. declaredProperties, .. inheritedProperties];
        _extensionDataProperty = extensionDataProperty;
    }

    /// <inheritdoc/>
    public Type Type
        => typeof(T);

    /// <inheritdoc/>
    public IFieldValueRecordModel? Parent
        => _parent;

    /// <inheritdoc/>
    public IFieldValueRecordConstructorModel<T>? Constructor 
        => _constructor;

    /// <inheritdoc/>
    public ImmutableArray<IFieldValueRecordPropertyModel<T>> Properties(bool includeInherited = true)
        => includeInherited
        ? _allProperties
        : _declaredProperties;

    /// <inheritdoc/>
    public IFieldValueRecordPropertyModel<T, JsonElement>? JsonExtensionDataProperty
        => _extensionDataProperty;

    private static IEnumerable<IFieldValueRecordPropertyModel<T>> GetDeclaredProperties()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var property in properties)
        {
            var propType = property.PropertyType;
            Type modelType;

            if (FieldValue.IsFieldValueType(propType, out var innerType))
            {
                modelType = typeof(FieldValuePropertyModel<,>).MakeGenericType([typeof(T), innerType]);
            }
            else
            {
                modelType = typeof(PropertyModel<,>).MakeGenericType([typeof(T), propType]);
            }

            var propertyModel = Activator.CreateInstance(modelType, property);
            Debug.Assert(propertyModel is IFieldValueRecordPropertyModel<T>);

            yield return (IFieldValueRecordPropertyModel<T>)propertyModel;
        }
    }

    private static IFieldValueRecordPropertyModel<T, JsonElement>? GetExtensionDataProperty(
        ref ImmutableArray<IFieldValueRecordPropertyModel<T>> declaredProperties,
        IFieldValueRecordModel? parent)
    {
        // first, check if any of the discovered properties is an extension-data property (and remove it from the set of properties if so)
        for (var i = 0; i < declaredProperties.Length; i++)
        {
            var property = declaredProperties[i];
            if (property.GetCustomAttribute<JsonExtensionDataAttribute>(inherit: true) is { } extensionAttribute)
            {
                if (property.IsUnsettable)
                {
                    ThrowHelper.ThrowInvalidOperationException(
                        $"Property '{property.Name}' of type '{property.MemberInfo.DeclaringType}' is marked with {nameof(JsonExtensionDataAttribute)} but is a FieldValue.");
                }

                if (property.Type != typeof(JsonElement))
                {
                    ThrowHelper.ThrowInvalidOperationException(
                        $"Property '{property.Name}' of type '{property.MemberInfo.DeclaringType}' is marked with {nameof(JsonExtensionDataAttribute)} but is not of type '{nameof(JsonElement)}'.");
                }

                declaredProperties = declaredProperties.RemoveAt(i);
                return (IFieldValueRecordPropertyModel<T, JsonElement>)property;
            }
        }

        // next, check if there are any private properties that are marked with JsonExtensionDataAttribute
        foreach (var property in typeof(T).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (property.GetCustomAttribute<JsonExtensionDataAttribute>(inherit: true) is { } extensionAttribute)
            {
                if (property.PropertyType != typeof(JsonElement))
                {
                    ThrowHelper.ThrowInvalidOperationException(
                        $"Property '{property.Name}' of type '{property.DeclaringType}' is marked with {nameof(JsonExtensionDataAttribute)} but is not of type '{nameof(JsonElement)}'.");
                }

                var modelType = typeof(PropertyModel<,>).MakeGenericType([typeof(T), property.PropertyType]);
                var propertyModel = Activator.CreateInstance(modelType, property);
                Debug.Assert(propertyModel is IFieldValueRecordPropertyModel<T, JsonElement>);

                return (IFieldValueRecordPropertyModel<T, JsonElement>)propertyModel;
            }
        }

        // next, check if there are any declared fields or private properties that are marked with JsonExtensionDataAttribute
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (field.GetCustomAttribute<JsonExtensionDataAttribute>(inherit: true) is { } extensionAttribute)
            {
                if (field.FieldType != typeof(JsonElement))
                {
                    ThrowHelper.ThrowInvalidOperationException(
                        $"Field '{field.Name}' of type '{field.DeclaringType}' is marked with {nameof(JsonExtensionDataAttribute)} but is not of type '{nameof(JsonElement)}'.");
                }

                var modelType = typeof(FieldModel<,>).MakeGenericType([typeof(T), field.FieldType]);
                var fieldModel = Activator.CreateInstance(modelType, field);
                Debug.Assert(fieldModel is IFieldValueRecordPropertyModel<T, JsonElement>);

                return (IFieldValueRecordPropertyModel<T, JsonElement>)fieldModel;
            }
        }

        // lastly, check the parent model (if any)
        if (parent?.JsonExtensionDataProperty is { } parentProp)
        {
            return (IFieldValueRecordPropertyModel<T, JsonElement>)parentProp;
        }

        return null;
    }

    private static IFieldValueRecordConstructorModel<T>? FindConstructor()
    {
        if (typeof(T).IsAbstract)
        {
            return null;
        }

        var ctors = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        // first, check if any of the constructors is decorated with JsonConstructorAttribute
        var jsonCtor = ctors.FirstOrDefault(c => c.GetCustomAttribute<JsonConstructorAttribute>() is not null);
        if (jsonCtor is not null)
        {
            return CreateConstructorModel(jsonCtor);
        }

        if (ctors.Length == 0)
        {
            return ThrowHelper.ThrowInvalidOperationException<IFieldValueRecordConstructorModel<T>>("No public constructors found.");
        }

        // check if there's only 1 public constructor
        if (ctors.Length == 1)
        {
            return CreateConstructorModel(ctors[0]);
        }

        // check if there is a public parameterless constructor
        var parameterlessCtor = ctors.FirstOrDefault(c => c.GetParameters().Length == 0);
        if (parameterlessCtor is not null)
        {
            return CreateConstructorModel(parameterlessCtor);
        }

        // multiple candidates, none of them are parameterless, and none of them are decorated with JsonConstructorAttribute
        return ThrowHelper.ThrowInvalidOperationException<IFieldValueRecordConstructorModel<T>>("Multiple constructors found, but none of them are decorated with JsonConstructorAttribute.");

        static IFieldValueRecordConstructorModel<T> CreateConstructorModel(ConstructorInfo ctor)
        {
            var parameters = ctor
                .GetParameters()
                .Select(static p =>
                {
                    var parameterType = p.ParameterType;
                    Type modelType;

                    if (FieldValue.IsFieldValueType(parameterType, out var innerType))
                    {
                        modelType = typeof(FieldValueConstructorParameterModel<,>).MakeGenericType([typeof(T), innerType]);
                    }
                    else
                    {
                        modelType = typeof(ConstructorParameterModel<,>).MakeGenericType([typeof(T), parameterType]);
                    }

                    var parameterModel = Activator.CreateInstance(modelType, p);
                    Debug.Assert(parameterModel is IFieldValueRecordConstructorParameterModel);

                    return (IFieldValueRecordConstructorParameterModel)parameterModel;
                })
                .ToImmutableArray();

            return new ConstructorModel<T>(ctor, parameters);
        }
    }
}
