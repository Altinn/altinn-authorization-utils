using System.Collections;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

/// <summary>
/// Represents a read-only collection of scopes that can be checked against a scope string.
/// </summary>
public abstract class ScopeSearchValues
    : IReadOnlyList<string>
{
    /// <summary>
    /// Creates a new instance of <see cref="ScopeSearchValues"/> using the specified set of string values.
    /// </summary>
    /// <param name="values">A read-only span of strings representing the values to be used for scope-based searching. Must contain at least
    /// one element.</param>
    /// <returns>A <see cref="ScopeSearchValues"/> instance initialized with the provided values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    public static ScopeSearchValues Create(scoped ReadOnlySpan<string> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values cannot be empty", nameof(values));
        }

        return new RegexScopeSearchValues(ImmutableArray.Create(values));
    }

    private readonly ImmutableArray<string> _values;

    private ScopeSearchValues(ImmutableArray<string> values)
    {
        _values = values;
    }

    /// <inheritdoc/>
    public string this[int index] => _values[index];

    /// <inheritdoc/>
    public int Count => _values.Length;

    /// <inheritdoc cref="ImmutableArray{T}.GetEnumerator()"/>
    public ImmutableArray<string>.Enumerator GetEnumerator()
        => _values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)_values).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)_values).GetEnumerator();

    /// <summary>
    /// Determines whether the specified scope-string contains any of the values in this collection.
    /// </summary>
    /// <param name="scopeString">A space-separated list of scopes.</param>
    /// <returns>Whether or not any of scopes in this collection was contained in <paramref name="scopeString"/>.</returns>
    public abstract bool Check(ReadOnlySpan<char> scopeString);

    private sealed class RegexScopeSearchValues
        : ScopeSearchValues
    {
        private readonly Regex _regex;

        public RegexScopeSearchValues(ImmutableArray<string> values)
            : base(values)
        {
            // preceeded by either whitespace or start of string
            var pattern = new StringBuilder("(?<=^| )(");

            var first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    pattern.Append('|');
                }

                first = false;
                pattern.Append(Regex.Escape(value));
            }

            // succeeded by either whitespace or end of string
            pattern.Append(")(?=$| )");

            _regex = new Regex(pattern.ToString(), RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        }

        public override bool Check(ReadOnlySpan<char> scopeString)
            => _regex.IsMatch(scopeString);
    }
}
