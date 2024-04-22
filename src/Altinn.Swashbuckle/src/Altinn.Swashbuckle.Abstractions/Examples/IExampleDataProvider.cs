namespace Altinn.Swashbuckle.Examples;

public interface IExampleDataProvider<TSelf>
    where TSelf : IExampleDataProvider<TSelf>
{
    /// <summary>
    /// Gets examples of the type.
    /// </summary>
    /// <returns>The example data.</returns>
    public static abstract IEnumerable<TSelf>? GetExamples(ExampleDataOptions options);
}
