using System.Collections.Immutable;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extension methods for <see cref="ProblemDescriptor"/>, <see cref="ProblemInstance"/>, <see cref="ValidationErrorDescriptor"/>, 
/// <see cref="ValidationErrorInstance"/>, and <see cref="ValidationErrorBuilder"/>.
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

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => ValidationErrorInstance.Create(descriptor, paths);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, path, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, path, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, ImmutableArray{KeyValuePair{string, string}})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ImmutableArray<KeyValuePair<string, string>> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);
    #endregion
}
