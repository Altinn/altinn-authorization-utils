namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Represents extensions for the command invocation context.
/// </summary>
public interface IContextExtensions
{
    /// <summary>
    /// Gets or sets a given feature. Setting a null value removes the feature.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The requested feature, or null if it is not present.</returns>
    object? this[Type key] { get; set; }

    /// <summary>
    /// Retrieves the requested feature from the collection.
    /// </summary>
    /// <typeparam name="TFeature">The feature key.</typeparam>
    /// <returns>The requested feature, or null if it is not present.</returns>
    TFeature? Get<TFeature>();

    /// <summary>
    /// Sets the given feature in the collection.
    /// </summary>
    /// <typeparam name="TFeature">The feature key.</typeparam>
    /// <param name="instance">The feature value.</param>
    void Set<TFeature>(TFeature? instance);

    /// <summary>
    /// Retrieves the requested feature from the collection, or adds it if it is not present.
    /// </summary>
    /// <typeparam name="TFeature">The feature type.</typeparam>
    /// <typeparam name="TFactoryArg">The type of the factory argument.</typeparam>
    /// <param name="arg">The factory argument.</param>
    /// <param name="factory">The factory function to create the feature if it is not present.</param>
    /// <returns>The requested feature.</returns>
    TFeature GetOrAdd<TFeature, TFactoryArg>(TFactoryArg arg, Func<TFactoryArg, TFeature> factory);

    /// <summary>
    /// Retrieves the requested feature from the collection, or adds it if it is not present.
    /// </summary>
    /// <typeparam name="TFeature">The feature type.</typeparam>
    /// <param name="factory">The factory function to create the feature if it is not present.</param>
    /// <returns>The requested feature.</returns>
    TFeature GetOrAdd<TFeature>(Func<TFeature> factory)
        => GetOrAdd(factory, static factory => factory());
}
