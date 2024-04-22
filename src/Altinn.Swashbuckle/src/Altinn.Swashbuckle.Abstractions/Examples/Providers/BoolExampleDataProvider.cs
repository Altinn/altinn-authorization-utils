namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class BoolExampleDataProvider
    : ExampleDataProvider<bool>
{
    /// <inheritdoc/>
    public override IEnumerable<bool> GetExamples(ExampleDataOptions options)
        => [true, false];
}
