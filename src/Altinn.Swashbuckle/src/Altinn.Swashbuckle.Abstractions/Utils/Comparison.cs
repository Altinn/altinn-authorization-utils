using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Altinn.Swashbuckle.Utils;

[DebuggerDisplay("{ToString(),nq}")]
internal readonly struct Comparison
    : IEquatable<Comparison>
    , IEqualityOperators<Comparison, Comparison, bool>
{
    public static Comparison LessThan => new(-1);
    public static Comparison Equal => new(0);
    public static Comparison GreaterThan => new(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Comparison From(int comparison)
        => comparison switch
        {
            < 0 => LessThan,
            0 => Equal,
            > 0 => GreaterThan,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Comparison Then<T>(T? left, T? right)
        where T : IComparable<T>
    {
        if (_value != 0)
        {
            return this;
        }

        return (left, right) switch
        {
            (null, null) => Equal,
            (null, _) => LessThan,
            (_, null) => GreaterThan,
            _ => From(left.CompareTo(right)),
        };
    }

    private readonly sbyte _value;

    public Comparison(sbyte value)
    {
        _value = value;
    }

    public override string ToString()
        => _value switch
        {
            < 0 => nameof(LessThan),
            0 => nameof(Equal),
            > 0 => nameof(GreaterThan),
        };

    public override int GetHashCode()
        => _value;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is Comparison other && Equals(other);

    public bool Equals(Comparison other)
        => _value == other._value;

    public static bool operator == (Comparison left, Comparison right)
        => left.Equals(right);

    public static bool operator != (Comparison left, Comparison right)
        => !left.Equals(right);

    public static implicit operator int(Comparison comparison)
        => comparison._value;
}
