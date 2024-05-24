using System.Collections;

namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// Provides example data for a type.
/// </summary>
public abstract class ExampleDataProvider
{
    /// <summary>
    /// Determines whether the type can be provided.
    /// </summary>
    /// <param name="typeToProvide">The type is checked as to whether it can be provided.</param>
    /// <returns><see langword="true"/> if the type can be provided, <see langword="false"/> otherwise.</returns>
    public abstract bool CanProvide(Type typeToProvide);

    /// <summary>
    /// Gets an example of the type.
    /// </summary>
    /// <param name="typeToProvide">The type to get an example of.</param>
    /// <param name="options">The options to use when getting the example.</param>
    /// <returns>The example data.</returns>
    public abstract IEnumerable? GetExamples(Type typeToProvide, ExampleDataOptions options);
}

/// <summary>
/// Provides example data for a type.
/// </summary>
/// <typeparam name="T">The type this provider can provide examples for.</typeparam>
public abstract class ExampleDataProvider<T>
    : ExampleDataProvider
{
    /// <inheritdoc/>
    public sealed override bool CanProvide(Type typeToProvide)
        => typeToProvide == typeof(T);

    /// <summary>
    /// Gets examples of the type.
    /// </summary>
    /// <returns>The example data.</returns>
    public abstract IEnumerable<T>? GetExamples(ExampleDataOptions options);

    /// <inheritdoc/>
    public sealed override IEnumerable? GetExamples(Type typeToProvide, ExampleDataOptions options)
        => GetExamples(options);
}
