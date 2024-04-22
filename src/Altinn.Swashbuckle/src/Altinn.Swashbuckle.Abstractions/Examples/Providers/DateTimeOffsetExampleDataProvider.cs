namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class DateTimeOffsetExampleDataProvider
    : ExampleDataProvider<DateTimeOffset>
{
    /// <inheritdoc/>
    public override IEnumerable<DateTimeOffset> GetExamples(ExampleDataOptions options)
        => [DateTimeOffset.UnixEpoch];
}
