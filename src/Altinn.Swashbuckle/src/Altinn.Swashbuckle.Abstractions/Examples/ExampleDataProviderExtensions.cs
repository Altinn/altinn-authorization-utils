using CommunityToolkit.Diagnostics;

namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// Extension methods for <see cref="ExampleDataProvider"/>.
/// </summary>
public static class ExampleDataProviderExtensions 
{
    /// <summary>
    /// Get a typed provider from a non-typed provider.
    /// </summary>
    /// <typeparam name="T">The type of example data.</typeparam>
    /// <param name="provider">The untyped <see cref="ExampleDataProvider"/>.</param>
    /// <returns>A <see cref="ExampleDataProvider{T}"/> where the type is <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">If the provider cannot provide example data of type <typeparamref name="T"/>.</exception>
    public static ExampleDataProvider<T> AsTypedProvider<T>(this ExampleDataProvider provider)
    {
        if (provider is ExampleDataProvider<T> casted)
        {
            return casted;
        }

        if (!provider.CanProvide(typeof(T)))
        {
            ThrowHelper.ThrowInvalidOperationException($"{provider.GetType().Name} cannot provide examples for type {typeof(T).Name}.");
        }

        return new ExampleDataProviderAdapter<T>(provider);
    }

    /// <summary>
    /// Map example data to a new type.
    /// </summary>
    /// <typeparam name="T">The original example data type.</typeparam>
    /// <typeparam name="U">The mapped example data type.</typeparam>
    /// <param name="provider">A <see cref="ExampleDataProvider{T}"/> that can provide example <typeparamref name="T"/>s.</param>
    /// <param name="selector">A function that converts a <typeparamref name="T"/> to a <typeparamref name="U"/>.</param>
    /// <returns>A <see cref="ExampleDataProvider{T}"/> for <typeparamref name="U"/>s.</returns>
    public static ExampleDataProvider<U> Select<T, U>(this ExampleDataProvider<T> provider, Func<T, U> selector)
        => new SelectExampleDataProvider<T, U>(provider, selector);

    private sealed class ExampleDataProviderAdapter<T>
        : ExampleDataProvider<T>
    {
        private readonly ExampleDataProvider _inner;

        public ExampleDataProviderAdapter(ExampleDataProvider inner)
        {
            _inner = inner;
        }

        public override IEnumerable<T>? GetExamples(ExampleDataOptions options)
            => _inner.GetExamples(typeof(T), options)?.Cast<T>();
    }

    private sealed class SelectExampleDataProvider<T, U>
        : ExampleDataProvider<U>
    {
        private readonly ExampleDataProvider<T> _inner;
        private readonly Func<T, U> _selector;

        public SelectExampleDataProvider(ExampleDataProvider<T> inner, Func<T, U> selector)
        {
            _inner = inner;
            _selector = selector;
        }

        public override IEnumerable<U>? GetExamples(ExampleDataOptions options)
            => _inner.GetExamples(options)?.Select(_selector);
    }
}
