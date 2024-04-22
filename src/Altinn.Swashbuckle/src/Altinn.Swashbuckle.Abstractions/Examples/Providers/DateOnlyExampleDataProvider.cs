namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class DateOnlyExampleDataProvider
    : ExampleDataProvider<DateOnly>
{
    /// <inheritdoc/>
    public override IEnumerable<DateOnly> GetExamples(ExampleDataOptions options)
        => [DateOnly.FromDateTime(DateTime.UnixEpoch)];
}
