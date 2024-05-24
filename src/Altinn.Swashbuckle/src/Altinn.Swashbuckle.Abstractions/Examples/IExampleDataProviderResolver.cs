namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// A resolver for getting example-data providers.
/// </summary>
public interface IExampleDataProviderResolver
{
    /// <summary>
    /// Gets an example-data provider for the type.
    /// </summary>
    ExampleDataProvider<T>? GetProvider<T>(ExampleDataOptions options)
    {
        var provider = GetProvider(typeof(T), options);

        return provider?.AsTypedProvider<T>();
    }

    /// <summary>
    /// Gets an example-data provider for the type.
    /// </summary>
    ExampleDataProvider? GetProvider(Type type, ExampleDataOptions options);
}
