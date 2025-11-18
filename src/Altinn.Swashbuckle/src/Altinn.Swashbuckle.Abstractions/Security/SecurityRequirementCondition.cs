using Altinn.Swashbuckle.Utils;
using CommunityToolkit.Diagnostics;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Represents a condition for a security requirement, consisting of an authentication scheme name and an optional
/// scope.
/// </summary>
/// <remarks>Use this type to specify the required authentication scheme and scope when describing security
/// requirements, such as those used in API authorization policies. Instances are immutable and can be compared for
/// equality and ordering. The <see cref="Create"/> method provides a convenient way to construct a new
/// instance.</remarks>
public sealed record SecurityRequirementCondition
    : IComparable<SecurityRequirementCondition>
    , IEquatable<SecurityRequirementCondition>
{
    /// <summary>
    /// Creates a new instance of the SecurityRequirementCondition class using the specified authentication scheme name
    /// and optional scope.
    /// </summary>
    /// <param name="schemeName">The name of the authentication scheme to associate with the security requirement. Cannot be null or empty.</param>
    /// <param name="scope">An optional scope to apply to the security requirement. If null, no scope is set.</param>
    /// <returns>A SecurityRequirementCondition instance configured with the specified scheme name and scope.</returns>
    public static SecurityRequirementCondition Create(string schemeName, string? scope = null)
    {
        if (scope is not null)
        {
            Guard.IsNotEmpty(scope);
        }

        return new(schemeName, scope);
    }

    private readonly string _schemeName;
    private readonly string? _scope;

    private SecurityRequirementCondition(string schemeName, string? scope)
    {
        _schemeName = schemeName;
        _scope = scope;
    }

    /// <summary>
    /// Gets the name of the authentication scheme associated with this instance.
    /// </summary>
    public string SchemeName => _schemeName;

    /// <summary>
    /// Gets the scope associated with the current context, if any.
    /// </summary>
    public string? Scope => _scope;

    /// <inheritdoc/>
    int IComparable<SecurityRequirementCondition>.CompareTo(SecurityRequirementCondition? other)
        => Comparison.Equal
            .Then(_schemeName, other?._schemeName)
            .Then(_scope, other?._scope);
}
