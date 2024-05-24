using System.Collections;

namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// Entry point for getting example data.
/// </summary>
public static class ExampleData 
{
    /// <summary>
    /// Gets the default options for getting example data.
    /// </summary>
    public static ExampleDataOptions DefaultOptions => ExampleDataOptions.DefaultOptions;

    /// <summary>
    /// Gets example data for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to get example data for.</typeparam>
    /// <param name="options">The options to use.</param>
    /// <returns>An enumerable of example data.</returns>
    public static IEnumerable<T>? GetExamples<T>(ExampleDataOptions? options = null)
        => GetExamples(typeof(T), options)?.Cast<T>();

    /// <summary>
    /// Gets example data for the specified type.
    /// </summary>
    /// <param name="type">The type to get example data for.</param>
    /// <param name="options">The options to use.</param>
    /// <returns>An enumerable of example data.</returns>
    public static IEnumerable? GetExamples(Type type, ExampleDataOptions? options = null)
    {
        options ??= DefaultOptions;

        var provider = options.GetProvider(type);

        return provider?.GetExamples(type, options);
    }
}
