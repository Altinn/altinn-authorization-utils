namespace Altinn.Swashbuckle.Examples;

public static class ExampleDataProviderExtensions 
{
    public static ExampleDataProvider<T> AsTypedProvider<T>(this ExampleDataProvider provider)
    {
        if (provider is ExampleDataProvider<T> casted)
        {
            return casted;
        }

        if (!provider.CanProvide(typeof(T)))
        {
            throw new InvalidOperationException($"{provider.GetType().Name} cannot provide examples for type {typeof(T).Name}.");
        }

        return new ExampleDataProviderAdapter<T>(provider);
    }

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
