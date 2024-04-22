namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class IExampleDataProviderProvider
    : ExampleDataProviderFactory
{
    /// <inheritdoc/>
    public override bool CanProvide(Type typeToProvide)
    {
        var ifaces = typeToProvide.GetInterfaces();
        return ifaces.Any(iface => iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IExampleDataProvider<>));
    }

    /// <inheritdoc/>
    protected internal override ExampleDataProvider CreateProvider(Type typeToProvide, ExampleDataOptions options)
    {
        return (ExampleDataProvider)Activator.CreateInstance(typeof(Provider<>).MakeGenericType(typeToProvide))!;
    }

    private class Provider<T>
        : ExampleDataProvider<T>
        where T : IExampleDataProvider<T>
    {
        /// <inheritdoc/>
        public override IEnumerable<T>? GetExamples(ExampleDataOptions options)
            => T.GetExamples(options);
    }
}
