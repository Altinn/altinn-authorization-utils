using System.Collections.Immutable;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extension methods for <see cref="ProblemDescriptor"/>, <see cref="ProblemInstance"/>, <see cref="ValidationErrorDescriptor"/>, 
/// <see cref="ValidationErrorInstance"/>, and <see cref="ValidationErrors"/>.
/// </summary>
public static class ProblemExtensions
{
    #region ProblemDescriptor.Create
    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor)"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor) 
        => ProblemInstance.Create(descriptor);

    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor, ImmutableArray<KeyValuePair<string, string>> extensions) 
        => ProblemInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor, ReadOnlySpan{KeyValuePair{string, string}})"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor, ReadOnlySpan<KeyValuePair<string, string>> extensions) 
        => ProblemInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor, IReadOnlyDictionary{string, string})"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor, IReadOnlyDictionary<string, string> extensions) 
        => ProblemInstance.Create(descriptor, extensions);
    #endregion

    #region ValidationErrorDescriptor.Create
    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor) 
        => ValidationErrorInstance.Create(descriptor);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path)
        => ValidationErrorInstance.Create(descriptor, path);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths)
        => ValidationErrorInstance.Create(descriptor, paths);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ReadOnlySpan{string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths)
        => ValidationErrorInstance.Create(descriptor, paths);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => ValidationErrorInstance.Create(descriptor, paths);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ReadOnlySpan{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, path, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, ReadOnlySpan{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, path, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, path, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, ReadOnlySpan{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ReadOnlySpan{string}, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ReadOnlySpan{string}, ReadOnlySpan{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ReadOnlySpan{string}, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, ReadOnlySpan{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);
    #endregion

    #region ValidationErrors.Add
    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <returns>A <see cref="ValidationErrorInstance"/>.</returns>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor)
        => errors.Add(descriptor.Create());

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, string path)
        => errors.Add(descriptor.Create(path));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths)
        => errors.Add(descriptor.Create(paths));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths)
        => errors.Add(descriptor.Create(paths));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => errors.Add(descriptor.Create(paths));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ImmutableArray<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, string path, ImmutableArray<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(path, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, string path, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(path, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="path"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="path">The path.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(path, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ImmutableArray<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, ImmutableArray<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ImmutableArray<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ReadOnlySpan<KeyValuePair<string, string>> extensions)
        => errors.Add(descriptor.Create(paths, extensions));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/>, <paramref name="paths"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="errors">The error collection.<param>
    /// <param name="descriptor">The <see cref="ValidationErrorDescriptor"/>.</param>
    /// <param name="paths">The paths.</param>
    /// <param name="extensions">The extensions.</param>
    public static void Add(this ref ValidationErrors errors, ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions)
        => errors.Add(descriptor.Create(paths, extensions));
    #endregion
}
