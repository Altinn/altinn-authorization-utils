using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
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

    private FieldValueRecordModel()
    {
        if (typeof(T).BaseType is { } baseType && baseType != typeof(object))
        {
            _parent = FieldValueRecordModel.For(baseType);
        }

        var ctor = FindConstructor();
        var declaredProperties = GetDeclaredProperties().ToImmutableArray();
        var inheritedProperties = _parent is null ? [] : _parent.Properties(includeInherited: true).CastArray<IFieldValueRecordPropertyModel<T>>();

        _constructor = ctor;
        _declaredProperties = declaredProperties;
        _allProperties = [.. declaredProperties, .. inheritedProperties];
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
