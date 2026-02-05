using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A utility for building validation errors.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
public struct MultipleProblemBuilder
    : ICollection<ProblemInstance>
    , IReadOnlyCollection<ProblemInstance>
{
    private CollectionBuilder<ProblemInstance> _problems;
    private ProblemExtensionDataBuilder _extensions;
    private string? _detail;

    /// <summary>
    /// Adds a problem instance.
    /// </summary>
    /// <param name="problem">The problem to add.</param>
    public void Add(ProblemInstance problem)
    {
        _problems.Add(problem);
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
    /// Gets the problem count.
    /// </summary>
    public readonly int Count => _problems.Count;

    /// <summary>
    /// Returns <see langword="true"/> if the collection is empty.
    /// </summary>
    public readonly bool IsEmpty
        => _problems.Count == 0;

    /// <inheritdoc/>
    bool ICollection<ProblemInstance>.IsReadOnly => false;

    /// <inheritdoc cref="IEnumerable{ProblemInstance}.GetEnumerator()"/>
    public readonly CollectionBuilderEnumerator<ProblemInstance> GetEnumerator()
        => _problems.GetEnumerator();

    /// <inheritdoc/>
    readonly IEnumerator<ProblemInstance> IEnumerable<ProblemInstance>.GetEnumerator()
        => _problems.GetEnumerator();

    /// <inheritdoc/>
    readonly IEnumerator IEnumerable.GetEnumerator()
        => _problems.GetEnumerator();

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> from this builder if any problems have been added.
    /// </summary>
    /// <param name="instance">The resulting <see cref="ProblemInstance"/>.</param>
    /// <returns>
    /// <see langword="true"/> if any validation errors have been added and the <paramref name="instance"/>
    /// has been created; otherwise <see langword="false"/>.
    /// </returns>
    public readonly bool TryBuild([NotNullWhen(true)] out ProblemInstance? instance)
    {
        if (_problems.Count == 0)
        {
            instance = null;
            return false;
        }

        if (_problems.Count == 1 && _extensions.Count == 0 && string.IsNullOrEmpty(Detail))
        {
            instance = _problems[0];
            return true;
        }

        ProblemExtensionData extensions = [];
        if (_extensions.Count > 0)
        {
            extensions = _extensions.ToImmutable();
        }

        instance = new MultipleProblemInstance(problems: _problems.ToImmutable(), Detail, extensions: extensions);
        return true;
    }

    /// <inheritdoc/>
    public void Clear()
        => _problems.Clear();

    /// <inheritdoc/>
    public bool Contains(ProblemInstance item)
        => _problems.Contains(item);

    /// <inheritdoc/>
    void ICollection<ProblemInstance>.CopyTo(ProblemInstance[] array, int arrayIndex)
        => _problems.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    bool ICollection<ProblemInstance>.Remove(ProblemInstance item)
        => _problems.Remove(item);
}
