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
    : IFieldValueRecordModel
{
    private static readonly ConcurrentDictionary<Type, FieldValueRecordModel> _cache = new();

    /// <inheritdoc/>
    public abstract Type Type { get; }

    /// <inheritdoc/>
    public abstract IFieldValueRecordModel? Parent { get; }

    /// <inheritdoc/>
    public abstract ImmutableArray<IFieldValueRecordPropertyModel> Properties(bool includeInherited = true);

    /// <inheritdoc/>
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

public interface IFieldValueRecordModel
{
    /// <summary>
    /// Gets the type of the record.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the parent model, if any.
    /// </summary>
    public IFieldValueRecordModel? Parent { get; }

    /// <summary>
    /// Gets the properties of the record.
    /// </summary>
    /// <param name="includeInherited">Whether or not to include inherited properties.</param>
    /// <returns>The properties of the model.</returns>
    public ImmutableArray<IFieldValueRecordPropertyModel> Properties(bool includeInherited = true);

    /// <summary>
    /// Gets the constructor of the record.
    /// </summary>
    public IFieldValueRecordConstructorModel Constructor { get; }
}

public interface IFieldValueRecordPropertyModelVisitor<out TOwner, TResult>
    where TOwner : class
{
    public TResult Visit<TValue>(IFieldValueRecordPropertyModel<TOwner, TValue> property)
        where TValue : notnull;
}

public interface IFieldValueRecordPropertyModel
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string Name { get; }

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
    /// Gets whether the property is nullable.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Gets a custom attribute of the specified type.
    /// </summary>
    /// <typeparam name="T">The attribute type.</typeparam>
    /// <param name="inherit">Whether to include inherite attributes.</param>
    /// <returns>A <typeparamref name="T"/>, if it was found.</returns>
    public T? GetCustomAttribute<T>(bool inherit)
        where T : Attribute;
}

public interface IFieldValueRecordPropertyModel<in TOwner>
    : IFieldValueRecordPropertyModel
    where TOwner : class
{
    public TResult Accept<TResult>(IFieldValueRecordPropertyModelVisitor<TOwner, TResult> visitor);
}

public interface IFieldValueRecordPropertyModel<in TOwner, TValue>
    : IFieldValueRecordPropertyModel<TOwner>
    where TOwner : class
    where TValue : notnull
{
    /// <summary>
    /// Reads the property value from the specified owner.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <returns>The property value.</returns>
    public FieldValue<TValue> Read(TOwner owner);

    /// <summary>
    /// Writes the property value to the specified owner.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="value">The value.</param>
    public void Write(TOwner owner, FieldValue<TValue> value);

    /// <inheritdoc/>
    Type IFieldValueRecordPropertyModel.Type => typeof(TValue);

    /// <inheritdoc/>
    TResult IFieldValueRecordPropertyModel<TOwner>.Accept<TResult>(IFieldValueRecordPropertyModelVisitor<TOwner, TResult> visitor)
        => visitor.Visit<TValue>(this);
}

public interface IFieldValueRecordConstructorModel
{
    /// <summary>
    /// Gets the parameters of the constructor.
    /// </summary>
    public ImmutableArray<IFieldValueRecordConstructorParameterModel> Parameters { get; }

    public object Invoke(Span<object?> parameters);
}

public interface IFieldValueRecordConstructorModel<out T>
    : IFieldValueRecordConstructorModel
    where T : notnull
{
    public new T Invoke(Span<object?> parameters);

    object IFieldValueRecordConstructorModel.Invoke(Span<object?> parameters)
        => Invoke(parameters);
}

public interface IFieldValueRecordConstructorParameterModel
{
    /// <summary>
    /// Gets the name of the constructor parameter.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the type of the constructor parameter.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the default value of the constructor parameter.
    /// </summary>
    public FieldValue<object> DefaultValue { get; }
}

public interface IFieldValueRecordConstructorParameterModel<in TOwner>
    : IFieldValueRecordConstructorParameterModel
    where TOwner : class
{
}

public interface IFieldValueRecordConstructorParameterModel<in TOwner, TValue>
    : IFieldValueRecordConstructorParameterModel<TOwner>
    where TOwner : class
    where TValue : notnull
{
    /// <summary>
    /// Gets the default value of the constructor parameter.
    /// </summary>
    public new FieldValue<TValue> DefaultValue { get; }

    FieldValue<object> IFieldValueRecordConstructorParameterModel.DefaultValue
        => DefaultValue.Select(static v => (object)v);
}
