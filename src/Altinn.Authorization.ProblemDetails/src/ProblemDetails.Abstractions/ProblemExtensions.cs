using System.Collections.Immutable;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extension methods for <see cref="ProblemDescriptor"/>, <see cref="ProblemInstance"/>, <see cref="ValidationErrorDescriptor"/>, 
/// and <see cref="ValidationErrorInstance"/>.
/// </summary>
public static class ProblemExtensions
{
    #region ProblemDescriptor.Create
    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor)"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor) 
        => ProblemInstance.Create(descriptor);

    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor, string?)"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor, string? detail)
        => ProblemInstance.Create(descriptor, detail);

    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor, ProblemExtensionData)"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor, ProblemExtensionData extensions) 
        => ProblemInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor, string?, ProblemExtensionData)"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor, string? detail, ProblemExtensionData extensions)
        => ProblemInstance.Create(descriptor, detail, extensions);

    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor, IReadOnlyDictionary{string, string})"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor, IReadOnlyDictionary<string, string> extensions) 
        => ProblemInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ProblemInstance.Create(ProblemDescriptor, string?, IReadOnlyDictionary{string, string})"/>
    public static ProblemInstance Create(this ProblemDescriptor descriptor, string? detail, IReadOnlyDictionary<string, string> extensions)
        => ProblemInstance.Create(descriptor, detail, extensions);
    #endregion

    #region ValidationErrorDescriptor.Create
    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor) 
        => ValidationErrorInstance.Create(descriptor);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path)
        => ValidationErrorInstance.Create(descriptor, path);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, string? detail)
        => ValidationErrorInstance.Create(descriptor, path, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths)
        => ValidationErrorInstance.Create(descriptor, paths);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, string? detail)
        => ValidationErrorInstance.Create(descriptor, paths, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => ValidationErrorInstance.Create(descriptor, paths);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, string? detail)
        => ValidationErrorInstance.Create(descriptor, paths, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ProblemExtensionData)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ProblemExtensionData extensions)
        => ValidationErrorInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ProblemExtensionData, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ProblemExtensionData extensions, string? detail)
        => ValidationErrorInstance.Create(descriptor, extensions, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IReadOnlyDictionary{string, string}, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IReadOnlyDictionary<string, string> extensions, string? detail)
        => ValidationErrorInstance.Create(descriptor, extensions, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, ProblemExtensionData)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions)
        => ValidationErrorInstance.Create(descriptor, path, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, ProblemExtensionData, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, ProblemExtensionData extensions, string? detail)
        => ValidationErrorInstance.Create(descriptor, path, extensions, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, path, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, string, IReadOnlyDictionary{string, string}, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, string path, IReadOnlyDictionary<string, string> extensions, string? detail)
        => ValidationErrorInstance.Create(descriptor, path, extensions, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, ProblemExtensionData)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ProblemExtensionData extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, ProblemExtensionData, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, ProblemExtensionData extensions, string? detail)
        => ValidationErrorInstance.Create(descriptor, paths, extensions, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, ImmutableArray{string}, IReadOnlyDictionary{string, string}, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, ImmutableArray<string> paths, IReadOnlyDictionary<string, string> extensions, string? detail)
        => ValidationErrorInstance.Create(descriptor, paths, extensions, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, ProblemExtensionData)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ProblemExtensionData extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, ProblemExtensionData, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ProblemExtensionData extensions, string? detail)
        => ValidationErrorInstance.Create(descriptor, paths, extensions, detail);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, IReadOnlyDictionary{string, string})"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions)
        => ValidationErrorInstance.Create(descriptor, paths, extensions);

    /// <inheritdoc cref="ValidationErrorInstance.Create(ValidationErrorDescriptor, IEnumerable{string}, IReadOnlyDictionary{string, string}, string?)"/>
    public static ValidationErrorInstance Create(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IReadOnlyDictionary<string, string> extensions, string? detail)
        => ValidationErrorInstance.Create(descriptor, paths, extensions, detail);
    #endregion
}
