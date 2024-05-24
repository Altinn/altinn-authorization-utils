
using System.Collections;
using System.Diagnostics;

namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// Provides example data providers for a type.
/// </summary>
public abstract class ExampleDataProviderFactory
    : ExampleDataProvider
{
    /// <summary>
    /// Creates a provider for the type.
    /// </summary>
    /// <param name="typeToProvide">The type to provide examples for.</param>
    /// <param name="options">The options to use.</param>
    /// <returns>An <see cref="ExampleDataProvider"/>.</returns>
    protected internal abstract ExampleDataProvider? CreateProvider(Type typeToProvide, ExampleDataOptions options);

    /// <inheritdoc/>
    public sealed override IEnumerable? GetExamples(Type typeToProvide, ExampleDataOptions options)
    {
        var provider = CreateProvider(typeToProvide, options);
        if (provider == null)
        {
            return null;
        }

        if (provider is ExampleDataProviderFactory)
        {
            throw new InvalidOperationException($"{GetType().Name} returned a factory for type {typeToProvide.Name}.");
        }

        Debug.Assert(provider.CanProvide(typeToProvide), $"{provider.GetType().Name} cannot provide examples for type {typeToProvide.Name}.");
        return provider.GetExamples(typeToProvide, options);
    }
}
