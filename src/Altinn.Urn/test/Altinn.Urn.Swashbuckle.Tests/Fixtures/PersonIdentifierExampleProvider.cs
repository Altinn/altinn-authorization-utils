using Altinn.Swashbuckle.Examples;

namespace Altinn.Urn.Swashbuckle.Tests.Fixtures;

public class PersonIdentifierExampleProvider
    : ExampleDataProvider<PersonIdentifier>
{
    public override IEnumerable<PersonIdentifier>? GetExamples(ExampleDataOptions options)
        => [PersonIdentifier.Parse("12345678901", null)];
}
