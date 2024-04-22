namespace Altinn.Swashbuckle.Examples.Providers;

internal sealed class TimeOnlyExampleDataProvider
    : ExampleDataProvider<TimeOnly>
{
    /// <inheritdoc/>
    public override IEnumerable<TimeOnly> GetExamples(ExampleDataOptions options)
        => [TimeOnly.FromDateTime(DateTime.UnixEpoch)];
}
