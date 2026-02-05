using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A utility for building validation errors.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
public struct ValidationProblemBuilder
    : ICollection<ValidationErrorInstance>
    , IReadOnlyCollection<ValidationErrorInstance>
{
    private CollectionBuilder<ValidationErrorInstance> _errors;
    private ProblemExtensionDataBuilder _extensions;
    private string? _detail;

    /// <summary>
    /// Adds a validation error.
    /// </summary>
    /// <param name="error">The error to add.</param>
    public void Add(ValidationErrorInstance error)
    {
        _errors.Add(error);
    }

    /// <summary>
    /// Adds an extension to the error (if one is created).
    /// </summary>
    /// <param name="key">Extension key.</param>
    /// <param name="value">Extension value.</param>
    public void AddExtension(string key, string value)
    {
        _extensions.Add(key, value);
    }

    /// <summary>
    /// Gets or sets the detailed description or additional information associated with the object.
    /// </summary>
    public string? Detail
    {
        readonly get => _detail;
        set => _detail = value;
    }

    /// <summary>
    /// Gets the error count.
    /// </summary>
    public readonly int Count => _errors.Count;

    /// <summary>
    /// Returns <see langword="true"/> if the collection is empty.
    /// </summary>
    public readonly bool IsEmpty
        => _errors.Count == 0;

    /// <inheritdoc/>
    bool ICollection<ValidationErrorInstance>.IsReadOnly => false;

    /// <inheritdoc cref="IEnumerable{ProblemInstance}.GetEnumerator()"/>
    public readonly CollectionBuilderEnumerator<ValidationErrorInstance> GetEnumerator()
        => _errors.GetEnumerator();

    /// <inheritdoc/>
    readonly IEnumerator<ValidationErrorInstance> IEnumerable<ValidationErrorInstance>.GetEnumerator()
        => _errors.GetEnumerator();

    /// <inheritdoc/>
    readonly IEnumerator IEnumerable.GetEnumerator()
        => _errors.GetEnumerator();

    /// <summary>
    /// Creates a new <see cref="ValidationProblemInstance"/> from this builder if any
    /// validation errors have been added.
    /// </summary>
    /// <param name="instance">The resulting <see cref="ValidationProblemInstance"/>.</param>
    /// <returns>
    /// <see langword="true"/> if any validation errors have been added and the <paramref name="instance"/>
    /// has been created; otherwise <see langword="false"/>.
    /// </returns>
    public readonly bool TryBuild([NotNullWhen(true)] out ValidationProblemInstance? instance)
    {
        if (_errors.Count == 0)
        {
            instance = null;
            return false;
        }

        ProblemExtensionData extensions = [];
        if (_extensions is { Count: > 0 })
        {
            extensions = _extensions.ToImmutable();
        }

        instance = new(errors: _errors.ToImmutable(), Detail, extensions: extensions);
        return true;
    }

    /// <inheritdoc/>
    public void Clear()
        => _errors.Clear();

    /// <inheritdoc/>
    public bool Contains(ValidationErrorInstance item)
        => _errors.Contains(item);

    /// <inheritdoc/>
    void ICollection<ValidationErrorInstance>.CopyTo(ValidationErrorInstance[] array, int arrayIndex)
        => _errors.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    bool ICollection<ValidationErrorInstance>.Remove(ValidationErrorInstance item)
        => _errors.Remove(item);
}
