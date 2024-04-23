using Altinn.Swashbuckle.Examples;

namespace Altinn.Urn.Swashbuckle.Tests.Fixtures;

public class PersonIdentifierExampleProviderResolver
    : IExampleDataProviderResolver
{
    public ExampleDataProvider? GetProvider(Type type, ExampleDataOptions options)
    {
        if (type == typeof(PersonIdentifier))
        {
            return new PersonIdentifierExampleProvider();
        }

        return null;
    }
}
