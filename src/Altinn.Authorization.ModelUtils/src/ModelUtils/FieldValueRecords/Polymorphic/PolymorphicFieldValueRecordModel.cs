using Altinn.Authorization.ModelUtils;
using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;

/// <summary>
/// Polymorphic field-value-record model factory.
/// </summary>
public static class PolymorphicFieldValueRecordModel
{
    private static readonly ConcurrentDictionary<Type, IPolymorphicFieldValueRecordModel> _cache = new();

    /// <summary>
    /// Gets the model for the specified type.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>A <see cref="FieldValueRecordModel{T}"/> for <typeparamref name="T"/>.</returns>
    public static IPolymorphicFieldValueRecordModel For<T>()
        where T : class
        => For(typeof(T));

    /// <summary>
    /// Gets the model for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A <see cref="FieldValueRecordModel"/> for <paramref name="type"/>.</returns>
    public static IPolymorphicFieldValueRecordModel For(Type type)
        => _cache.GetOrAdd(type, static t => CreateModel(t));

    private static IPolymorphicFieldValueRecordModel CreateModel(Type type)
    {
        var rootType = FindRoot(type);
        if (rootType != type)
        {
            return ((IPolymorphicRootFieldValueRecordModel)For(rootType)).ModelFor(type);
        }

        var model = FieldValueRecordModel.For(type);
        var discriminator = FindDiscriminator(model);

        if (!NonExhaustiveEnum.IsNonExhaustiveEnumType(discriminator.Type, out var discriminatorInnerType))
        {
            if (!discriminator.Type.IsEnum)
            {
                ThrowHelper.ThrowInvalidOperationException($"The type '{discriminator.Type}' (discriminator property for '{model.Type}') is not an enum, nor a non-exhaustive enum.");
            }

            discriminatorInnerType = discriminator.Type;
            var discriminatorModelType = typeof(NonExhaustiveDiscriminatorPropertyModelWrapper<,>).MakeGenericType(type, discriminatorInnerType);
            discriminator = (IFieldValueRecordPropertyModel)Activator.CreateInstance(discriminatorModelType, discriminator)!;
        }

        var rootModelType = typeof(PolymorphicRootFieldValueRecordModel<,>).MakeGenericType(type, discriminatorInnerType);
        object? rootModelUntyped;
        try
        {
            rootModelUntyped = Activator.CreateInstance(rootModelType, model, discriminator)!;
        }
        catch (TargetInvocationException e)
        {
            if (e.InnerException is { } inner)
            {
                ExceptionDispatchInfo.Throw(inner);
            }

            throw;
        }

        Debug.Assert(rootModelUntyped is IPolymorphicRootFieldValueRecordModel);

        var rootModel = (IPolymorphicRootFieldValueRecordModel)rootModelUntyped;
        return rootModel;

        static Type FindRoot(Type inType)
        {
            for (Type? type = inType; type is not null; type = type.BaseType)
            {
                var attr = type.GetCustomAttribute<PolymorphicFieldValueRecordAttribute>(inherit: false);

                if (attr is null)
                {
                    ThrowHelper.ThrowInvalidOperationException($"The type '{type}' (part of the hierarchy of '{inType}') is not a polymorphic field-value-record.");
                }

                if (attr.IsRoot)
                {
                    return type;
                }
            }

            return ThrowHelper.ThrowInvalidOperationException<Type>($"The type '{inType}' does not have a polymorphic root.");
        }

        static IFieldValueRecordPropertyModel FindDiscriminator(IFieldValueRecordModel model)
        {
            var discriminatorProps = model.Properties(includeInherited: false)
                //.Select(static prop => (Prop: prop, Attr: prop.GetCustomAttribute<PolymorphicTypePropertyAttribute>(inherit: false)))
                .Where(static prop => prop.GetCustomAttribute<PolymorphicDiscriminatorPropertyAttribute>(inherit: false) is not null);

            var enumerator = discriminatorProps.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                ThrowHelper.ThrowInvalidOperationException($"The type '{model.Type}' does not have a discriminator property.");
            }

            var prop = enumerator.Current;

            if (enumerator.MoveNext())
            {
                ThrowHelper.ThrowInvalidOperationException($"The type '{model.Type}' has more than one discriminator property.");
            }

            return prop;
        }
    }
}

internal sealed class PolymorphicFieldValueRecordModel<T, TDiscriminator>
    : IPolymorphicFieldValueRecordModel<TDiscriminator, T>
    where T : class
    where TDiscriminator : struct, Enum
{
    private readonly IPolymorphicRootFieldValueRecordModel _root;
    private readonly IFieldValueRecordModel<T> _recordModel;
    private readonly IFieldValueRecordPropertyModel<T, NonExhaustiveEnum<TDiscriminator>> _discriminatorProperty;
    private readonly ImmutableArray<TDiscriminator> _discriminators;
    private readonly ImmutableArray<IPolymorphicFieldValueRecordModel<TDiscriminator>> _descendants;
    private readonly FrozenDictionary<TDiscriminator, IPolymorphicFieldValueRecordModel<TDiscriminator>> _byDiscriminator;
    private readonly FrozenDictionary<Type, IPolymorphicFieldValueRecordModel<TDiscriminator>> _byType;
    private readonly ImmutableArray<IFieldValueRecordPropertyModel<T>> _selfProperties;
    private readonly ImmutableArray<IFieldValueRecordPropertyModel<T>> _allProperties;

    public PolymorphicFieldValueRecordModel(
        IPolymorphicRootFieldValueRecordModel root,
        IFieldValueRecordPropertyModel<T, NonExhaustiveEnum<TDiscriminator>> discriminatorProperty,
        IFieldValueRecordModel<T> recordModel,
        IEnumerable<TDiscriminator> discriminators,
        IEnumerable<IPolymorphicFieldValueRecordModel<TDiscriminator>> descendants)
    {
        _root = root;
        _recordModel = recordModel;
        _discriminatorProperty = discriminatorProperty;
        _descendants = descendants.ToImmutableArray();
        _discriminators = discriminators.ToImmutableArray();

        var allModels = _descendants.Concat([this]);
        
        _byDiscriminator = allModels
            .SelectMany(static model => model
                .Discriminators(includeDescendants: false)
                .Select(d => KeyValuePair.Create(d, model)))
            .ToFrozenDictionary();

        _byType = allModels.ToFrozenDictionary(static m => m.Type);

        _selfProperties = SortDiscriminatorFirst(recordModel.Properties(includeInherited: false), _discriminatorProperty);
        _allProperties = SortDiscriminatorFirst(recordModel.Properties(includeInherited: true), _discriminatorProperty);

        IsNonExhaustive = discriminatorProperty.MemberInfo is PropertyInfo pi && NonExhaustiveEnum.IsNonExhaustiveEnumType(pi.PropertyType, out _);
    }

    public bool IsNonExhaustive { get; }

    public IPolymorphicRootFieldValueRecordModel Root => _root;

    public IFieldValueRecordConstructorModel<T>? Constructor => _recordModel.Constructor;

    public Type Type => _recordModel.Type;

    public IFieldValueRecordModel? Parent => _recordModel.Parent;

    public ImmutableArray<IPolymorphicFieldValueRecordModel<TDiscriminator>> Descendants => _descendants;

    public IFieldValueRecordPropertyModel<T, NonExhaustiveEnum<TDiscriminator>> DiscriminatorProperty => _discriminatorProperty;

    public bool TryGetDescendantModel(Type type, [NotNullWhen(true)] out IPolymorphicFieldValueRecordModel<TDiscriminator>? model)
        => _byType.TryGetValue(type, out model);

    public bool TryGetDescendantModel(TDiscriminator discriminator, [NotNullWhen(true)] out IPolymorphicFieldValueRecordModel<TDiscriminator>? model)
        => _byDiscriminator.TryGetValue(discriminator, out model);

    public ImmutableArray<TDiscriminator> Discriminators(bool includeDescendants = true)
        => includeDescendants
        ? _byDiscriminator.Keys
        : _discriminators;

    public ImmutableArray<IFieldValueRecordPropertyModel<T>> Properties(bool includeInherited = true)
        => includeInherited
        ? _allProperties
        : _selfProperties;

    public IFieldValueRecordPropertyModel<T, JsonElement>? JsonExtensionDataProperty
        => _recordModel.JsonExtensionDataProperty;

    private static ImmutableArray<IFieldValueRecordPropertyModel<T>> SortDiscriminatorFirst(
        ImmutableArray<IFieldValueRecordPropertyModel<T>> properties,
        IFieldValueRecordPropertyModel<T> discriminatorProperty)
    {
        var index = -1;
        for (var i = 0; i < properties.Length; i++)
        {
            if (properties[i].MemberInfo == discriminatorProperty.MemberInfo)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            return properties;
        }

        var builder = ImmutableArray.CreateBuilder<IFieldValueRecordPropertyModel<T>>(properties.Length);
        builder.Add(properties[index]);

        for (var i = 0; i < properties.Length; i++)
        {
            if (i == index)
            {
                continue;
            }

            builder.Add(properties[i]);
        }

        return builder.MoveToImmutable();
    }
}
