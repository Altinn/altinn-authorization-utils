using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A utility for building validation errors.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
public struct MultipleProblemBuilder
    : IReadOnlyList<ProblemInstance>
{
    private List<ProblemInstance>? _problems;
    private List<KeyValuePair<string, string>>? _extensions;

    /// <summary>
    /// Adds a problem instance.
    /// </summary>
    /// <param name="problem">The problem to add.</param>
    public void Add(ProblemInstance problem)
    {
        _problems ??= new(8);
        _problems.Add(problem);
    }

    /// <summary>
    /// Adds an extension to the error (if one is created).
    /// </summary>
    /// <param name="key">Extension key.</param>
    /// <param name="value">Extension value.</param>
    public void AddExtension(string key, string value)
    {
        _extensions ??= new(8);
        _extensions.Add(new(key, value));
    }

    /// <summary>
    /// Gets the problem count.
    /// </summary>
    public readonly int Count => _problems?.Count ?? 0;

    /// <inheritdoc/>
    public readonly ProblemInstance this[int index]
        => _problems is null
        ? ThrowHelper.ThrowArgumentOutOfRangeException<ProblemInstance>()
        : _problems[index];

    /// <summary>
    /// Returns <see langword="true"/> if the collection is empty.
    /// </summary>
    public readonly bool IsEmpty
        => _problems switch
        {
            null => true,
            _ => _problems.Count == 0,
        };

    /// <inheritdoc cref="IEnumerable{ProblemInstance}.GetEnumerator()"/>
    public readonly IEnumerator<ProblemInstance> GetEnumerator()
        => _problems switch
        {
            null => Enumerable.Empty<ProblemInstance>().GetEnumerator(),
            _ => _problems.GetEnumerator(),
        };

    /// <inheritdoc/>
    readonly IEnumerator<ProblemInstance> IEnumerable<ProblemInstance>.GetEnumerator()
        => _problems switch
        {
            null => Enumerable.Empty<ProblemInstance>().GetEnumerator(),
            _ => _problems.GetEnumerator(),
        };

    /// <inheritdoc/>
    readonly IEnumerator IEnumerable.GetEnumerator()
        => _problems switch
        {
            null => Enumerable.Empty<ProblemInstance>().GetEnumerator(),
            _ => _problems.GetEnumerator(),
        };

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> from this builder if any problems have been added.
    /// </summary>
    /// <param name="instance">The resulting <see cref="ProblemInstance"/>.</param>
    /// <returns>
    /// <see langword="true"/> if any validation errors have been added and the <paramref name="instance"/>
    /// has been created; otherwise <see langword="false"/>.
    /// </returns>
    public readonly bool TryBuild([NotNullWhen(true)] out ProblemInstance? instance)
        => TryBuild(detail: null, out instance);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> from this builder if any problems have been added.
    /// </summary>
    /// <param name="detail">The error detail.</param>
    /// <param name="instance">The resulting <see cref="ProblemInstance"/>.</param>
    /// <returns>
    /// <see langword="true"/> if any validation errors have been added and the <paramref name="instance"/>
    /// has been created; otherwise <see langword="false"/>.
    /// </returns>
    public readonly bool TryBuild(string? detail, [NotNullWhen(true)] out ProblemInstance? instance)
    {
        var problems = _problems;
        if (problems is null or { Count: 0 })
        {
            instance = null;
            return false;
        }

        if (problems.Count == 1 && _extensions is null or { Count: 0 } && string.IsNullOrEmpty(detail))
        {
            instance = problems[0];
            return true;
        }

        ProblemExtensionData extensions = [];
        if (_extensions is { Count: > 0 })
        {
            extensions = [.. _extensions];
        }

        instance = new MultipleProblemInstance(problems: [.. problems], detail, extensions: extensions);
        return true;
    }
}
