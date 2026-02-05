using System.Collections;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Enumerator for <see cref="ProblemExtensionDataBuilder"/>, <see cref="MultipleProblemBuilder"/>, and <see cref="ValidationErrorBuilder"/>.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public struct CollectionBuilderEnumerator<T>
    : IEnumerator<T>
    where T : IEquatable<T>
{
    private CollectionBuilder<T> _builder;
    private int _index;

    internal CollectionBuilderEnumerator(in CollectionBuilder<T> builder)
    {
        _builder = builder;
        _index = -1;
    }

    /// <inheritdoc/>
    public readonly T Current => _builder[_index];

    /// <inheritdoc/>
    readonly object IEnumerator.Current => _builder[_index];

    /// <inheritdoc/>
    public void Dispose() 
    { 
        _index = _builder.Count;
    }

    /// <inheritdoc/>
    public bool MoveNext()
    {
        _index++;
        return _index < _builder.Count;
    }

    /// <inheritdoc/>
    public void Reset() 
        => _index = -1;
}
