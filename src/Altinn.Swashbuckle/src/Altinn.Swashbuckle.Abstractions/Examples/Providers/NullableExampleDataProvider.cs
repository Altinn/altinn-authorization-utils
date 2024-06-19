using System.Diagnostics;

namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class NullableExampleDataProvider
    : ExampleDataProviderFactory
{
    public override bool CanProvide(Type typeToProvide)
        => Nullable.GetUnderlyingType(typeToProvide) is not null;

    protected internal override ExampleDataProvider? CreateProvider(Type typeToProvide, ExampleDataOptions options)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToProvide)!;
        var underlyingProvider = options.GetProvider(underlyingType);

        if (underlyingProvider is null)
        {
            return null;
        }

        Debug.Assert(underlyingProvider.CanProvide(underlyingType));
        return (ExampleDataProvider)Activator.CreateInstance(typeof(Provider<>).MakeGenericType(underlyingType), [underlyingProvider])!;
    }

    private sealed class Provider<T>
        : ExampleDataProvider<T?>
        where T : struct
    {
        private readonly ExampleDataProvider<T> _underlyingProvider;

        public Provider(ExampleDataProvider underlyingProvider)
        {
            _underlyingProvider = underlyingProvider.AsTypedProvider<T>();
        }

        public override IEnumerable<T?>? GetExamples(ExampleDataOptions options)
            => _underlyingProvider.GetExamples(options)?.Select(value => (T?)value).Append(null);
    }
}
