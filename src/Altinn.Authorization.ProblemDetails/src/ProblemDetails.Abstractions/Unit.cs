using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Represents an empty value, similar to <see langword="void"/>, except that it can be used as a type argument.
/// </summary>
public readonly struct Unit
    : IEquatable<Unit>
    , IEqualityOperators<Unit, Unit, bool>
{
    /// <summary>
    /// Gets the singleton value of <see cref="Unit"/>.
    /// </summary>
    public static readonly Unit Value = default;

    /// <inheritdoc/>
    public override int GetHashCode() => 0;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Unit;

    /// <inheritdoc/>
    public bool Equals(Unit other) => true;

    /// <inheritdoc/>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <inheritdoc/>
    public static bool operator !=(Unit left, Unit right) => false;
}
