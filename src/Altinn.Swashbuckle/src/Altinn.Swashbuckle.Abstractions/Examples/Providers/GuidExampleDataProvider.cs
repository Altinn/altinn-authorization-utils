namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class GuidExampleDataProvider
    : ExampleDataProvider<Guid>
{
    private static readonly Guid _example = new Guid("049d6f78-f087-41c3-a4ec-c98f5451e387");

    /// <inheritdoc/>
    public override IEnumerable<Guid> GetExamples(ExampleDataOptions options)
        => [_example];
}
