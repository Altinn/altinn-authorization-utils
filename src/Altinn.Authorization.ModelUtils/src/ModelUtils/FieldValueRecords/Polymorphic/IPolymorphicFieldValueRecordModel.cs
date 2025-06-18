using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;

/// <summary>
/// A model for a polymorphic record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
public interface IPolymorphicFieldValueRecordModel
    : IFieldValueRecordModel
{
    /// <summary>
    /// Gets a value indicating whether this model is non-exhaustive.
    /// </summary>
    public bool IsNonExhaustive { get; }

    /// <summary>
    /// Gets the type of the discriminator.
    /// </summary>
    public Type DiscriminatorType { get; }

    /// <summary>
    /// Gets discriminators valid for this model.
    /// </summary>
    /// <param name="includeDescendants">Whether or not to include discriminators for descendant models.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of discriminator values, boxed as <see langword="object"/>s.</returns>
    public IEnumerable<object> Discriminators(bool includeDescendants = true);

    /// <summary>
    /// Gets the discriminator property model.
    /// </summary>
    public IFieldValueRecordPropertyModel DiscriminatorProperty { get; }

    /// <summary>
    /// Gets the root model.
    /// </summary>
    public IPolymorphicRootFieldValueRecordModel Root { get; }

    /// <summary>
    /// Gets a value indicating whether this model is the root model.
    /// </summary>
    public bool IsRoot => ReferenceEquals(this, Root);

    /// <summary>
    /// Gets the descendants of this model.
    /// </summary>
    public ImmutableArray<IPolymorphicFieldValueRecordModel> Descendants { get; }

    /// <summary>
    /// Tries to get the model from the type of a descendant.
    /// </summary>
    /// <param name="type">The descendant type.</param>
    /// <param name="model">The resulting descendant <see cref="IPolymorphicFieldValueRecordModel"/>, if found.</param>
    /// <returns><see langword="true"/>, if <paramref name="type"/> represents a descendant type and a model is found, otherwise <see langword="false"/>.</returns>
    public bool TryGetDescendantModel(Type type, [NotNullWhen(true)] out IPolymorphicFieldValueRecordModel? model);
}

/// <summary>
/// A model for a polymorphic record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
/// <typeparam name="TDiscriminator">The discriminator type.</typeparam>
public interface IPolymorphicFieldValueRecordModel<TDiscriminator>
    : IPolymorphicFieldValueRecordModel
    where TDiscriminator : struct, Enum
{
    /// <summary>
    /// Gets discriminators valid for this model.
    /// </summary>
    /// <param name="includeDescendants">Whether or not to include discriminators for descendant models.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <typeparamref name="TDiscriminator"/>.</returns>
    public new ImmutableArray<TDiscriminator> Discriminators(bool includeDescendants = true);

    /// <inheritdoc cref="IPolymorphicFieldValueRecordModel.Descendants"/>
    public new ImmutableArray<IPolymorphicFieldValueRecordModel<TDiscriminator>> Descendants { get; }

    /// <summary>
    /// Tries to get the model from the type of a descendant.
    /// </summary>
    /// <param name="type">The descendant type.</param>
    /// <param name="model">The resulting descendant <see cref="IPolymorphicFieldValueRecordModel{TDiscriminator}"/>, if found.</param>
    /// <returns><see langword="true"/>, if <paramref name="type"/> represents a descendant type and a model is found, otherwise <see langword="false"/>.</returns>
    public bool TryGetDescendantModel(Type type, [NotNullWhen(true)] out IPolymorphicFieldValueRecordModel<TDiscriminator>? model);

    /// <summary>
    /// Tries to get the model from the discriminator value of a descendant.
    /// </summary>
    /// <param name="discriminator">The discriminator value.</param>
    /// <param name="model">The resulting descendant <see cref="IPolymorphicFieldValueRecordModel{TDiscriminator}"/>, if found.</param>
    /// <returns><see langword="true"/>, if <paramref name="discriminator"/> is a discriminator for a descendant type and a model is found, otherwise <see langword="false"/>.</returns>
    public bool TryGetDescendantModel(TDiscriminator discriminator, [NotNullWhen(true)] out IPolymorphicFieldValueRecordModel<TDiscriminator>? model);

    /// <inheritdoc/>
    Type IPolymorphicFieldValueRecordModel.DiscriminatorType
        => typeof(TDiscriminator);

    /// <inheritdoc/>
    IEnumerable<object> IPolymorphicFieldValueRecordModel.Discriminators(bool includeDescendants)
        => Discriminators(includeDescendants).Cast<object>();

    /// <inheritdoc/>
    ImmutableArray<IPolymorphicFieldValueRecordModel> IPolymorphicFieldValueRecordModel.Descendants
        => ImmutableArray<IPolymorphicFieldValueRecordModel>.CastUp(Descendants);

    /// <inheritdoc/>
    bool IPolymorphicFieldValueRecordModel.TryGetDescendantModel(Type type, [NotNullWhen(true)] out IPolymorphicFieldValueRecordModel? model)
        => TryGetDescendantModel(type, out model);
}

/// <summary>
/// A model for a polymorphic record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
/// <typeparam name="TDiscriminator">The discriminator type.</typeparam>
/// <typeparam name="T">The type of the record.</typeparam>
public interface IPolymorphicFieldValueRecordModel<TDiscriminator, T>
    : IPolymorphicFieldValueRecordModel<TDiscriminator>
    , IFieldValueRecordModel<T>
    where TDiscriminator : struct, Enum
    where T : class
{
    /// <summary>
    /// Gets the discriminator property model.
    /// </summary>
    public new IFieldValueRecordPropertyModel<T, NonExhaustiveEnum<TDiscriminator>> DiscriminatorProperty { get; }

    /// <inheritdoc/>
    IFieldValueRecordPropertyModel IPolymorphicFieldValueRecordModel.DiscriminatorProperty
        => DiscriminatorProperty;
}
