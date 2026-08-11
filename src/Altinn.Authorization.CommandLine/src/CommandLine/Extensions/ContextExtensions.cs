using System.Collections.Concurrent;
using Altinn.Authorization.CommandLine.Factory;

namespace Altinn.Authorization.CommandLine.Extensions;

/// <summary>
/// Represents extensions for the command invocation context.
/// </summary>
public sealed class ContextExtensions
    : IContextExtensions
{
    private readonly ConcurrentDictionary<Type, object?> _features = new();

    /// <inheritdoc/>
    public object? this[Type key]
    {
        get => _features.TryGetValue(key, out var value) ? value : null;
        set => _features[key] = value;
    }

    /// <inheritdoc/>
    public TFeature? Get<TFeature>()
    {
        if (typeof(TFeature).IsValueType)
        {
            var feature = this[typeof(TFeature)];
            if (feature is null && Nullable.GetUnderlyingType(typeof(TFeature)) is null)
            {
                throw new InvalidOperationException(
                    $"{typeof(TFeature).FullName} does not exist in the feature collection " +
                    $"and because it is a struct the method can't return null. Use 'extensions[typeof({TypeNameHelper.GetTypeDisplayName(typeof(TFeature), fullName: false)})] is not null' to check if the feature exists.");
            }
            return (TFeature?)feature;
        }

        return (TFeature?)this[typeof(TFeature)];
    }

    /// <inheritdoc/>
    public void Set<TFeature>(TFeature? instance)
    {
        this[typeof(TFeature)] = instance;
    }

    /// <inheritdoc/>
    public TFeature GetOrAdd<TFeature, TFactoryArg>(TFactoryArg arg, Func<TFactoryArg, TFeature> factory)
    {
        return (TFeature)_features.AddOrUpdate(
            typeof(TFeature),
            static (_, factory) => factory.Invoke(),
            static (_, old, factory) => old is TFeature ? old : factory.Invoke(),
            new FactoryArg<TFeature, TFactoryArg>(arg, factory))!;
    }

    private readonly struct FactoryArg<TFeature, TFactoryArg>(TFactoryArg arg, Func<TFactoryArg, TFeature> factory)
    {
        public TFeature Invoke() => factory(arg);
    }
}
