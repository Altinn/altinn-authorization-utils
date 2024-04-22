namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class DateTimeExampleDataProvider
    : ExampleDataProvider<DateTime>
{
    /// <inheritdoc/>
    public override IEnumerable<DateTime> GetExamples(ExampleDataOptions options)
        => [DateTime.UnixEpoch];
}
