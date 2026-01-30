using System.Collections.Immutable;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extensions for <see cref="ValidationErrorBuilder"/>.
/// </summary>
public static class ValidationErrorBuilderExtensions
{
    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor)
        => errors.Add(descriptor.Create());

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, string path)
        => errors.Add(descriptor.Create(path));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, string path, string? detail)
        => errors.Add(descriptor.Create(path, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths)
        => errors.Add(descriptor.Create(paths));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, string? detail)
        => errors.Add(descriptor.Create(paths, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => errors.Add(descriptor.Create(paths));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths, string? detail)
        => errors.Add(descriptor.Create(paths, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, ProblemExtensionData extensions)
        => errors.Add(descriptor.Create(extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="detail">The error detail.</param>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, ProblemExtensionData extensions, string? detail)
        => errors.Add(descriptor.Create(extensions, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="detail">The error detail.</param>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions, string? detail)
        => errors.Add(descriptor.Create(extensions, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions)
        => errors.Add(descriptor.Create(path, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <param name="detail">The error detail.</param>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions, string? detail)
        => errors.Add(descriptor.Create(path, extensions, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(path, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <param name="detail">The error detail.</param>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions, string? detail)
        => errors.Add(descriptor.Create(path, extensions, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ProblemExtensionData extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ProblemExtensionData extensions, string? detail)
        => errors.Add(descriptor.Create(paths, extensions, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions, string? detail)
        => errors.Add(descriptor.Create(paths, extensions, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ProblemExtensionData extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ProblemExtensionData extensions, string? detail)
        => errors.Add(descriptor.Create(paths, extensions, detail));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.</param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public static void Add(this ref ValidationErrorBuilder errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions, string? detail)
        => errors.Add(descriptor.Create(paths, extensions, detail));
}
