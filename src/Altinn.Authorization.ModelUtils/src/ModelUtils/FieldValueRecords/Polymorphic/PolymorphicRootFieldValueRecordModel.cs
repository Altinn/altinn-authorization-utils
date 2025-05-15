using Altinn.Authorization.ModelUtils;
using Altinn.Authorization.ModelUtils.Internal;
using CommunityToolkit.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;

internal sealed class PolymorphicRootFieldValueRecordModel<T, TDiscriminator>
    : IPolymorphicRootFieldValueRecordModel<TDiscriminator, T>
    where T : class
    where TDiscriminator : struct, Enum
{
    /// <summary>
    /// Gets the singleton instance of the model.
    /// </summary>
    private readonly PolymorphicFieldValueRecordModel<T, TDiscriminator> _root;

    public PolymorphicRootFieldValueRecordModel(
        IFieldValueRecordModel<T> model,
        IFieldValueRecordPropertyModel<T, NonExhaustiveEnum<TDiscriminator>> discriminator)
    {
        var states = ResolveDiscriminators(model);
        _root = RealizeModels(this, discriminator, model, states);

        static PolymorphicFieldValueRecordModel<T, TDiscriminator> RealizeModels(
            PolymorphicRootFieldValueRecordModel<T, TDiscriminator> self,
            IFieldValueRecordPropertyModel<T, NonExhaustiveEnum<TDiscriminator>> discriminator,
            IFieldValueRecordModel<T> root,
            List<BuilderState> remaining)
        {
            Dictionary<Type, IPolymorphicFieldValueRecordModel<TDiscriminator>> models = new(remaining.Count);

            while (remaining.Count > 0)
            {
                var processedAny = false;
                for (var i = 0; i < remaining.Count; i++)
                {
                    var item = remaining[i];

                    // make sure all children are processed first
                    if (!item.Children.TrueForAll(child => models.ContainsKey(child.Type)))
                    {
                        continue;
                    }

                    processedAny = true;
                    var children = item.Descendants().Select(child => models[child.Type]);

                    var modelType = typeof(PolymorphicFieldValueRecordModel<,>).MakeGenericType(item.Type, typeof(TDiscriminator));
                    var untyped = Activator.CreateInstance(modelType, self, discriminator, item.RecordModel, item.Discriminators, children)!;
                    models.Add(item.Type, (IPolymorphicFieldValueRecordModel<TDiscriminator>)untyped);

                    remaining.SwapRemoveAt(i);
                    i--;
                }

                if (!processedAny)
                {
                    ThrowHelper.ThrowInvalidOperationException($"Failed to process DAG of polymorphic field-value-records based on '{root.Type}'.");
                }
            }

            return (PolymorphicFieldValueRecordModel<T, TDiscriminator>)models[root.Type];
        }

        static List<BuilderState> ResolveDiscriminators(IFieldValueRecordModel model)
        {
            var builder = new Dictionary<Type, BuilderState>();
            var seenDiscriminators = new HashSet<TDiscriminator>();

            builder.Add(model.Type, new(model));

            foreach (var knownTypeAttr in model.Type.GetCustomAttributes<PolymorphicDerivedTypeAttribute>(inherit: false))
            {
                var discriminatorValue = knownTypeAttr.TypeDiscriminator switch
                {
                    TDiscriminator value => value,
                    _ => ThrowHelper.ThrowInvalidOperationException<TDiscriminator>($"The value '{knownTypeAttr.TypeDiscriminator}' is not a valid discriminator value for '{knownTypeAttr.Type}'."),
                };

                var type = knownTypeAttr.Type;
                if (!type.IsAssignableTo(model.Type))
                {
                    ThrowHelper.ThrowInvalidOperationException($"Known type '{type}' is not assignable to '{model.Type}'.");
                }

                ref var builderEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(builder, type, out var exists);
                if (!exists)
                {
                    builderEntry = new(FieldValueRecordModel.For(type));
                }

                builderEntry!.Discriminators.Add(discriminatorValue);

                if (!seenDiscriminators.Add(discriminatorValue))
                {
                    ThrowHelper.ThrowInvalidOperationException($"The discriminator value '{discriminatorValue}' is already used by the type '{model.Type}'.");
                }
            }

            foreach (var state in builder.Values)
            {
                if (ReferenceEquals(state.RecordModel, model))
                {
                    continue;
                }

                if (builder.TryGetValue(state.Type.BaseType!, out var parentState))
                {
                    state.ParentState = parentState;
                    parentState.Children.Add(state);
                }
                else
                {
                    ThrowHelper.ThrowInvalidOperationException($"The type '{state.Type}' has parent '{state.Type.BaseType!}' which is not part of the polymorphic field-value-record hierarchy.");
                }
            }

            return [.. builder.Values];
        }
    }

    public IPolymorphicRootFieldValueRecordModel Root => this;

    public IFieldValueRecordConstructorModel<T> Constructor => _root.Constructor;

    public Type Type => _root.Type;

    public IFieldValueRecordModel? Parent => _root.Parent;

    public ImmutableArray<IPolymorphicFieldValueRecordModel<TDiscriminator>> Descendants => _root.Descendants;

    public IFieldValueRecordPropertyModel<T, NonExhaustiveEnum<TDiscriminator>> DiscriminatorProperty => _root.DiscriminatorProperty;

    public ImmutableArray<TDiscriminator> Discriminators(bool includeInherited = true)
        => _root.Discriminators(includeInherited);

    public IPolymorphicFieldValueRecordModel ModelFor(Type type)
        => _root.TryGetDescendantModel(type, out var model) 
        ? model 
        : ThrowHelper.ThrowInvalidOperationException<IPolymorphicFieldValueRecordModel>($"The type '{type}' is not part of the polymorphic field-value-record hierarchy.");

    public ImmutableArray<IFieldValueRecordPropertyModel<T>> Properties(bool includeInherited = true)
        => _root.Properties(includeInherited);

    public bool TryGetDescendantModel(Type type, [NotNullWhen(true)] out IPolymorphicFieldValueRecordModel<TDiscriminator>? model)
        => _root.TryGetDescendantModel(type, out model);

    public bool TryGetDescendantModel(TDiscriminator discriminator, [NotNullWhen(true)] out IPolymorphicFieldValueRecordModel<TDiscriminator>? model)
        => _root.TryGetDescendantModel(discriminator, out model);

    private class BuilderState(IFieldValueRecordModel model)
    {
        public Type Type => model.Type;

        public IFieldValueRecordModel RecordModel => model;

        public List<TDiscriminator> Discriminators { get; } = [];

        public BuilderState? ParentState { get; set; }

        public List<BuilderState> Children { get; } = [];

        public IEnumerable<BuilderState> Descendants()
        {
            foreach (var child in Children)
            {
                yield return child;

                foreach (var descendant in child.Descendants())
                {
                    yield return descendant;
                }
            }
        }
    }
}
