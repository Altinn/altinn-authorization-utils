using Altinn.Swashbuckle.Examples.Providers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// Default implementation of <see cref="IExampleDataProviderResolver"/>.
/// </summary>
public class DefaultExampleDataProviderResolver
    : IExampleDataProviderResolver
{
    internal static DefaultExampleDataProviderResolver DefaultInstance { get; } = new DefaultExampleDataProviderResolver();

    private static ImmutableArray<ExampleDataProvider> _builtinProviders = [
        new BoolExampleDataProvider(),
        new GuidExampleDataProvider(),
        new DateTimeExampleDataProvider(),
        new DateTimeOffsetExampleDataProvider(),
        new DateOnlyExampleDataProvider(),
        new TimeOnlyExampleDataProvider(),
    ];

    private static ImmutableArray<ExampleDataProviderFactory> _builtinFactories = [
        new NumberExampleDataProvider(),
        new NullableExampleDataProvider(),
        new ExampleDataProviderProvider(),
    ];

    /// <inheritdoc/>
    public ExampleDataProvider? GetProvider(Type type, ExampleDataOptions options)
    {
        if (options.ProvidersInternal is { } list)
        {
            foreach (var provider in list)
            {
                if (!provider.CanProvide(type))
                {
                    continue;
                }

                if (provider is ExampleDataProviderFactory factory)
                {
                    var constructedProvider = factory.CreateProvider(type, options);

                    if (constructedProvider is not null)
                    {
                        Debug.Assert(constructedProvider.CanProvide(type));
                        Debug.Assert(constructedProvider is not ExampleDataProviderFactory);

                        return constructedProvider;
                    }

                    continue;
                }

                return provider;
            }
        }

        foreach (var provider in _builtinProviders)
        {
            if (provider.CanProvide(type))
            {
                return provider;
            }
        }

        foreach (var factory in _builtinFactories)
        {
            if (!factory.CanProvide(type))
            {
                continue;
            }

            var constructedProvider = factory.CreateProvider(type, options);

            if (constructedProvider is not null)
            {
                Debug.Assert(constructedProvider.CanProvide(type));
                Debug.Assert(constructedProvider is not ExampleDataProviderFactory);

                return constructedProvider;
            }
        }

        return null;
    }
}
