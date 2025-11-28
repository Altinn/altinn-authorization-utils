using System.Collections;
using System.Collections.Immutable;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Represents a read-only, ordered collection of security requirements for an API operation or resource.
/// </summary>
public sealed class SecurityInfo
    : IReadOnlyList<SecurityRequirement>
{
    /// <summary>
    /// Creates a new instance of the <see cref="SecurityInfo"/> class using the specified collection of security
    /// requirements.
    /// </summary>
    /// <remarks>The returned <see cref="SecurityInfo"/> will contain requirements in a distinct and ordered
    /// form. If the input collection contains duplicates or is unordered, these will be normalized
    /// automatically.</remarks>
    /// <param name="requirements">An enumerable collection of <see cref="SecurityRequirement"/> objects that define the security requirements to
    /// include. Duplicate and unordered requirements are normalized.</param>
    /// <returns>A <see cref="SecurityInfo"/> instance containing the normalized set of security requirements.</returns>
    public static SecurityInfo Create(IEnumerable<SecurityRequirement> requirements)
    {
        var reqArray = requirements
            .Distinct()
            .Order()
            .ToImmutableArray();

        var normalized = Normalize(reqArray);
        return new(reqArray, normalized);
    }

    private readonly ImmutableArray<SecurityRequirement> _requirements;
    private readonly ImmutableArray<ImmutableArray<KeyValuePair<string, ImmutableArray<string>>>> _normalized;

    /// <inheritdoc/>
    public int Count 
        => _requirements.Length;

    /// <inheritdoc/>
    public SecurityRequirement this[int index] 
        => _requirements[index];

    private SecurityInfo(
        ImmutableArray<SecurityRequirement> requirements,
        ImmutableArray<ImmutableArray<KeyValuePair<string, ImmutableArray<string>>>> normalized)
    {
        _requirements = requirements;
        _normalized = normalized;
    }

    /// <summary>
    /// Get the OpenAPI-normalized representation of the security requirements.
    /// </summary>
    /// <returns>A list of <c>Dictionary&lt;SecurityScheme, ScopeName[]&gt;</c></returns>
    public ImmutableArray<ImmutableArray<KeyValuePair<string, ImmutableArray<string>>>> Normalized()
        => _normalized;

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()" />
    public ImmutableArray<SecurityRequirement>.Enumerator GetEnumerator()
        => _requirements.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<SecurityRequirement> IEnumerable<SecurityRequirement>.GetEnumerator()
        => ((IEnumerable<SecurityRequirement>)_requirements).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() 
        => ((IEnumerable)_requirements).GetEnumerator();

    // Distributing AND over OR, i.e. converting a logical formula from CNF (Conjunctive Normal Form) to DNF (Disjunctive Normal Form).
    // Example:
    //   Input: (A OR B) AND (C OR D)
    //   Output: (A AND C) OR (A AND D) OR (B AND C) OR (B AND D)
    private static ImmutableArray<ImmutableArray<KeyValuePair<string, ImmutableArray<string>>>> Normalize(ImmutableArray<SecurityRequirement> requirements)
    {
        var results = new List<HashSet<(string Scheme, string? Scope)>> { new() };
        
        foreach (var requirement in requirements)
        {
            var newResults = new List<HashSet<(string Scheme, string? Scope)>>();
            foreach (var partialResult in results)
            {
                foreach (var candidate in requirement)
                {
                    newResults.Add([.. partialResult, (candidate.SchemeName, candidate.Scope)]);
                }
            }

            results = newResults;
        }

        results = Reduce(results);

        return results
            .Select(static partialResult => partialResult
                .GroupBy(static rule => rule.Scheme, static kvp => kvp.Scope)
                .Select(static group => KeyValuePair.Create<string, ImmutableArray<string>>(group.Key, group.Where(static v => v is not null).Distinct().Order().ToImmutableArray()!))
                .OrderBy(static kvp => kvp.Key)
                .ToImmutableArray())
            .Where(static dict => dict.Length > 0)
            .ToImmutableArray();

        static List<HashSet<(string Scheme, string? Scope)>> Reduce(List<HashSet<(string Scheme, string? Scope)>> orList)
        {
            var simplified = new HashSet<HashSet<(string Scheme, string? Scope)>>(new SetComparer());

            for (int i = 0; i < orList.Count; i++)
            {
                var current = orList[i];
                bool subsumed = orList.Any(other => other != current && other.IsProperSubsetOf(current)); // other ⊆ current

                if (!subsumed)
                {
                    simplified.Add(current);
                }
            }

            // Remove supersets (redundant rules)
            var result = simplified
                .Where(a => !simplified.Any(b => b != a && a.IsProperSubsetOf(b)))
                .ToList();

            return result;
        }
    }

    private sealed class SetComparer
        : IEqualityComparer<HashSet<(string Scheme, string? Scope)>>
    {
        public bool Equals(HashSet<(string Scheme, string? Scope)>? x, HashSet<(string Scheme, string? Scope)>? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            if (x.Count != y.Count) return false;
            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<(string Scheme, string? Scope)>? obj)
        {
            if (obj is null)
            {
                return 0;
            }

            HashCode hash = default;
            
            foreach (var item in obj.OrderBy(i => i.Scheme).ThenBy(i => i.Scope))
            {
                hash.Add(item.Scheme);
                hash.Add(item.Scope);
            }

            return hash.ToHashCode();
        }
    }
}
