

using System.Numerics;

namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class NumberExampleDataProvider
    : ExampleDataProviderFactory
{
    /// <inheritdoc/>
    public override bool CanProvide(Type typeToProvide)
    {
        var ifaces = typeToProvide.GetInterfaces();
        return ifaces.Any(iface => iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(INumberBase<>));
    }

    /// <inheritdoc/>
    protected internal override ExampleDataProvider CreateProvider(Type typeToProvide, ExampleDataOptions options)
    {
        return (ExampleDataProvider)Activator.CreateInstance(typeof(Provider<>).MakeGenericType(typeToProvide))!;
    }

    private sealed class Provider<T>
        : ExampleDataProvider<T>
        where T : INumberBase<T>
    {
        /// <inheritdoc/>
        public override IEnumerable<T> GetExamples(ExampleDataOptions options)
            => [T.CreateTruncating(123), T.CreateTruncating(-123)];
    }
}
