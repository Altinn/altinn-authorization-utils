using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A utility for building validation errors.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
public struct ValidationErrors
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
    /// Converts the validation errors to an immutable array of <typeparamref name="T"/>s.
    /// </summary>
    /// <typeparam name="T">The type to map errors to.</typeparam>
    /// <param name="mapper">The mapper.</param>
    /// <returns>A <see cref="ImmutableArray{T}"/> of <typeparamref name="T"/>s.</returns>
    public readonly ImmutableArray<T> MapToImmutable<T>(Func<ValidationErrorInstance, T> mapper)
    {
        var errors = _errors;
        if (errors is null)
        {
            return [];
        }

        var builder = ImmutableArray.CreateBuilder<T>(errors.Count);
        foreach (var error in errors)
        {
            builder.Add(mapper(error));
        }

        return builder.MoveToImmutable();
    }
}
