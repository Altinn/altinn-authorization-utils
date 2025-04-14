using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
public abstract class FieldValueRecordModel
{
    private static readonly ConcurrentDictionary<Type, FieldValueRecordModel> _cache = new();

    /// <summary>
    /// Gets the type of the record.
    /// </summary>
    public abstract Type Type { get; }

    /// <summary>
    /// Gets the parent model, if any.
    /// </summary>
    public abstract FieldValueRecordModel? Parent { get; }

    /// <summary>
    /// Gets the properties of the record.
    /// </summary>
    /// <param name="includeInherited">Whether or not to include inherited properties.</param>
    /// <returns>The properties of the model.</returns>
    public abstract ImmutableArray<IFieldValueRecordPropertyModel> Properties(bool includeInherited = true);

    /// <summary>
    /// Gets the constructor of the record.
    /// </summary>
    public abstract IFieldValueRecordConstructorModel Constructor { get; }

    /// <summary>
    /// Gets the model for the specified type.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>A <see cref="FieldValueRecordModel{T}"/> for <typeparamref name="T"/>.</returns>
    public static FieldValueRecordModel<T> For<T>()
        where T : class
        => (FieldValueRecordModel<T>)For(typeof(T));

    /// <summary>
    /// Gets the model for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A <see cref="FieldValueRecordModel"/> for <paramref name="type"/>.</returns>
    public static FieldValueRecordModel For(Type type)
        => _cache.GetOrAdd(type, static t => CreateModel(t));

    private static FieldValueRecordModel CreateModel(Type type)
    {
        var modelType = typeof(FieldValueRecordModel<>).MakeGenericType(type);
        var instanceProperty = modelType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        Debug.Assert(instanceProperty is not null);

        var model = instanceProperty.GetValue(null);
        Debug.Assert(model is FieldValueRecordModel);

        return (FieldValueRecordModel)model;
    }
}

/// <summary>
/// A model for a record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
/// <typeparam name="T">The type of the record.</typeparam>
public sealed class FieldValueRecordModel<T>
    : FieldValueRecordModel
    where T : class
{
    /// <summary>
    /// Gets the singleton instance of the model.
    /// </summary>
    public static FieldValueRecordModel<T> Instance { get; } = new();

    private readonly FieldValueRecordModel? _parent;
    private readonly IFieldValueRecordConstructorModel<T> _constructor;
    private readonly ImmutableArray<IFieldValueRecordPropertyModel> _declaredProperties;
    private readonly ImmutableArray<IFieldValueRecordPropertyModel> _allProperties;

    private FieldValueRecordModel()
    {
        if (typeof(T).BaseType is { } baseType && baseType != typeof(object))
        {
            _parent = For(baseType);
        }

        var ctor = FindConstructor();
        var declaredProperties = GetDeclaredProperties().ToImmutableArray();
        var inheritedProperties = _parent is null ? [] : _parent.Properties(includeInherited: true);

        _constructor = ctor;
        _declaredProperties = declaredProperties;
        _allProperties = [.. declaredProperties, .. inheritedProperties];
    }

    /// <inheritdoc/>
    public override Type Type
        => typeof(T);

    /// <inheritdoc/>
    public override FieldValueRecordModel? Parent
        => _parent;

    /// <inheritdoc/>
    public override IFieldValueRecordConstructorModel<T> Constructor 
        => _constructor;

    /// <inheritdoc/>
    public override ImmutableArray<IFieldValueRecordPropertyModel> Properties(bool includeInherited = true)
        => includeInherited
        ? _allProperties
        : _declaredProperties;

    private static IEnumerable<IFieldValueRecordPropertyModel> GetDeclaredProperties()
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
            Debug.Assert(propertyModel is IFieldValueRecordPropertyModel);

            yield return (IFieldValueRecordPropertyModel)propertyModel;
        }
    }

    private static IFieldValueRecordConstructorModel<T> FindConstructor()
    {
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
