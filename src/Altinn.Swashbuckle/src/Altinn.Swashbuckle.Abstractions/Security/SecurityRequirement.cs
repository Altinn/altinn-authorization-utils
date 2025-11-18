using Altinn.Authorization.ModelUtils;
using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Models a security requirement for an API operation.
/// </summary>
[DebuggerDisplay("{_display,nq}, Count = {_conditions.Length,nq}")]
public sealed record SecurityRequirement
    : IReadOnlyList<SecurityRequirementCondition>
    , IComparable<SecurityRequirement>
{
    /// <summary>
    /// Creates a new instance of the <see cref="SecurityRequirement"/> class using the specified display name and a
    /// collection of conditions, any which will fulfill the requiremnt.
    /// </summary>
    /// <remarks>The returned security requirement will contain only unique conditions, ordered according to
    /// their natural ordering. If <paramref name="conditions"/> contains duplicates, only one instance of each will be
    /// included.</remarks>
    /// <param name="display">The display name to associate with the security requirement. Cannot be null.</param>
    /// <param name="conditions">A collection of candidate security requirements to include. Duplicate conditions are ignored. Cannot be null.</param>
    /// <returns>A <see cref="SecurityRequirement"/> initialized with the specified display name and the distinct, ordered set of
    /// conditions.</returns>
    public static SecurityRequirement Create(string display, IEnumerable<SecurityRequirementCondition> conditions)
    {
        var options = conditions
            .Distinct()
            .Order()
            .ToImmutableValueArray();

        if (options.IsEmpty)
        {
            ThrowHelper.ThrowArgumentException(
                "At least one security requirement condition must be specified.",
                nameof(conditions));
        }

        return new(display, options);
    }

    private readonly string _display;
    private readonly ImmutableValueArray<SecurityRequirementCondition> _conditions;

    /// <inheritdoc/>
    public int Count 
        => _conditions.Length;

    /// <inheritdoc/>
    public SecurityRequirementCondition this[int index]
        => _conditions[index];

    private SecurityRequirement(string display, ImmutableValueArray<SecurityRequirementCondition> candidates)
    {
        _display = display;
        _conditions = candidates;
    }

    /// <summary>
    /// Gets the display text associated with the current instance.
    /// </summary>
    public string Display
        => _display;

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
    public ImmutableArray<SecurityRequirementCondition>.Enumerator GetEnumerator()
        => _conditions.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<SecurityRequirementCondition> IEnumerable<SecurityRequirementCondition>.GetEnumerator()
        => ((IEnumerable<SecurityRequirementCondition>)_conditions).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)_conditions).GetEnumerator();

    /// <inheritdoc/>
    int IComparable<SecurityRequirement>.CompareTo(SecurityRequirement? other)
    {
        if (other is null)
        {
            return 1;
        }

        for (var i = 0; i < _conditions.Length; i++)
        {
            if (i >= other._conditions.Length)
            {
                return 1;
            }

            var selfItem = _conditions[i];
            var otherItem = other._conditions[i];
            var comparison = ((IComparable<SecurityRequirementCondition>)selfItem).CompareTo(otherItem);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        if (_conditions.Length < other._conditions.Length)
        {
            return -1;
        }

        return string.Compare(_display, other._display, StringComparison.Ordinal);
    }
}
