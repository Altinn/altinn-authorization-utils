namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extensions for <see cref="MultipleProblemBuilder"/>.
/// </summary>
public static class MultipleProblemBuilderExtensions
{
    /// <summary>
    /// Adds a problem with the specified <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="builder">The error collection.</param>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The error detail.</param>
    /// <param name="source">The source problem instance.</param>
    public static void Add(
        this ref MultipleProblemBuilder builder,
        ProblemDescriptor descriptor,
        string? detail = null,
        ProblemInstance? source = null)
        => builder.Add(descriptor.Create(detail, source));

    /// <summary>
    /// Adds a problem with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="builder">The error collection.</param>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="source">The source problem instance.</param>
    public static void Add(
        this ref MultipleProblemBuilder builder,
        ProblemDescriptor descriptor,
        ProblemExtensionData extensions,
        ProblemInstance? source = null)
        => builder.Add(descriptor.Create(extensions, source: source));

    /// <summary>
    /// Adds a problem with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="builder">The error collection.</param>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The error detail.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="source">The source problem instance.</param>
    public static void Add(
        this ref MultipleProblemBuilder builder,
        ProblemDescriptor descriptor,
        string? detail,
        ProblemExtensionData extensions,
        ProblemInstance? source = null)
        => builder.Add(descriptor.Create(detail, extensions, source: source));

    /// <summary>
    /// Adds a problem with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="builder">The error collection.</param>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="source">The source problem instance.</param>
    public static void Add(
        this ref MultipleProblemBuilder builder,
        ProblemDescriptor descriptor,
        IReadOnlyDictionary<string, string> extensions,
        ProblemInstance? source = null)
        => builder.Add(descriptor.Create(extensions, source: source));

    /// <summary>
    /// Adds a problem with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="builder">The error collection.</param>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The error detail.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="source">The source problem instance.</param>
    public static void Add(
        this ref MultipleProblemBuilder builder,
        ProblemDescriptor descriptor,
        string? detail,
        IReadOnlyDictionary<string, string> extensions,
        ProblemInstance? source = null)
        => builder.Add(descriptor.Create(detail, extensions, source: source));
}
