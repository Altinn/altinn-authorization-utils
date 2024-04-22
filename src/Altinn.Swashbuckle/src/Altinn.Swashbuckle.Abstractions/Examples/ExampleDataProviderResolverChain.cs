using Altinn.Swashbuckle.Configuration;
using CommunityToolkit.Diagnostics;

namespace Altinn.Swashbuckle.Examples;

internal class ExampleDataProviderResolverChain
    : ConfigurationList<IExampleDataProviderResolver>
    , IExampleDataProviderResolver
{
    public ExampleDataProviderResolverChain() : base(null) { }

    public override bool IsReadOnly => true;

    protected override void OnCollectionModifying()
        => ThrowHelper.ThrowInvalidDataException("The resolver chain is read-only.");

    internal void AddFlattened(IExampleDataProviderResolver? resolver)
    {
        switch (resolver)
        {
            case null:
                break;

            case ExampleDataProviderResolverChain otherChain:
                _list.AddRange(otherChain);
                break;

            default:
                _list.Add(resolver);
                break;
        }
    }

    public ExampleDataProvider? GetProvider(Type type, ExampleDataOptions options)
    {
        foreach (IExampleDataProviderResolver resolver in _list)
        {
            ExampleDataProvider? provider = resolver.GetProvider(type, options);
            if (provider != null)
            {
                return provider;
            }
        }

        return null;
    }
}
