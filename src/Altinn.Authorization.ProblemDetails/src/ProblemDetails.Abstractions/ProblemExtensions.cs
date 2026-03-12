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

    #region ValidationErrorInstance.ToProblemInstance
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/summary"/>
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/param[@name='validationError']"/>
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/returns"/>
    public static ValidationProblemInstance ToProblemInstance(this ValidationErrorInstance validationError)
        => validationError.ToProblemInstance(detail: null, extensions: default);

    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/summary"/>
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/param[@name='detail']"/>
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/param[@name='validationError']"/>
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/returns"/>
    public static ValidationProblemInstance ToProblemInstance(this ValidationErrorInstance validationError, string? detail)
        => validationError.ToProblemInstance(detail, extensions: default);

    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/summary"/>
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/param[@name='validationError']"/>
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/param[@name='extensions']"/>
    /// <inheritdoc cref="ToProblemInstance(ValidationErrorInstance, string?, ProblemExtensionData)" path="/returns"/>
    public static ValidationProblemInstance ToProblemInstance(this ValidationErrorInstance validationError, ProblemExtensionData extensions)
        => validationError.ToProblemInstance(detail: null, extensions);

    /// <summary>
    /// Creates a new <see cref="ValidationProblemInstance"/> containing the provided <see cref="ValidationErrorInstance"/> and additional details and extensions.
    /// </summary>
    /// <param name="validationError">The <see cref="ValidationErrorInstance"/>.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A new <see cref="ValidationProblemInstance"/>.</returns>
    public static ValidationProblemInstance ToProblemInstance(this ValidationErrorInstance validationError, string? detail, ProblemExtensionData extensions)
        => new([validationError], detail: detail, extensions: extensions);
    #endregion
}
