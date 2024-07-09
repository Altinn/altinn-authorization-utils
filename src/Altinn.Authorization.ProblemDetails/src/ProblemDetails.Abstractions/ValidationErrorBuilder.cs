using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A utility for building validation errors.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
public struct ValidationErrorBuilder
    : IReadOnlyCollection<ValidationErrorInstance>
{
    private List<ValidationErrorInstance>? _errors;

    /// <summary>
    /// Adds a validation error.
    /// </summary>
    /// <param name="error">The error to add.</param>
    public void Add(ValidationErrorInstance error)
    {
        _errors ??= new(8);
        _errors.Add(error);
    }

    /// <summary>
    /// Gets the error count.
    /// </summary>
    public readonly int Count => _errors?.Count ?? 0;

    /// <summary>
    /// Returns <see langword="true"/> if the collection is empty.
    /// </summary>
    public readonly bool IsEmpty 
        => _errors switch
        {
            null => true,
            _ => _errors.Count == 0,
        };

    /// <inheritdoc cref="IEnumerable{ValidationErrorInstance}.GetEnumerator()"/>
    public readonly IEnumerator<ValidationErrorInstance> GetEnumerator()
        => _errors switch
        {
            null => Enumerable.Empty<ValidationErrorInstance>().GetEnumerator(),
            _ => _errors.GetEnumerator(),
        };

    /// <inheritdoc/>
    IEnumerator<ValidationErrorInstance> IEnumerable<ValidationErrorInstance>.GetEnumerator()
        => _errors switch
        {
            null => Enumerable.Empty<ValidationErrorInstance>().GetEnumerator(),
            _ => _errors.GetEnumerator(),
        };

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => _errors switch
        {
            null => Enumerable.Empty<ValidationErrorInstance>().GetEnumerator(),
            _ => _errors.GetEnumerator(),
        };

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
        var errors = _errors;
        if (errors is null or { Count: 0 })
        {
            instance = null;
            return false;
        }

        instance = new(errors: [.. errors], extensions: []);
        return true;
    }
}
