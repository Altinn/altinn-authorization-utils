namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// An interface for providing example data for self.
/// </summary>
/// <typeparam name="TSelf">The self type.</typeparam>
public interface IExampleDataProvider<TSelf>
    where TSelf : IExampleDataProvider<TSelf>
{
    /// <summary>
    /// Gets examples of the type.
    /// </summary>
    /// <returns>The example data.</returns>
    public static abstract IEnumerable<TSelf>? GetExamples(ExampleDataOptions options);
}
