using Altinn.Swashbuckle.Examples;
using System.Collections.Immutable;

namespace Altinn.Urn.Swashbuckle;

/// <summary>
/// Provides example data for the UrnEncoded type using URN-encoded representations.
/// </summary>
internal sealed class UrnEncodedExampleDataProvider
    : ExampleDataProvider<UrnEncoded>
{
    private static readonly ImmutableArray<UrnEncoded> Examples = [
        UrnEncoded.Create("Olsen, Åge?=:"),
        UrnEncoded.Create("12345678901"),
        UrnEncoded.Create("ABC"),
    ];

    public override IEnumerable<UrnEncoded>? GetExamples(ExampleDataOptions options)
        => Examples;
}
