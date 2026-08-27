using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A wrapper type that ignores equality comparison. All instances of this type are considered equal, regardless of the underlying value.
/// This is useful to include in records to exclude certain fields from equality comparison, such as exceptions or other non-deterministic data.
/// </summary>
/// <typeparam name="T">The underlying type.</typeparam>
internal readonly struct EqualityIgnoredOptional<T>
    : IEquatable<EqualityIgnoredOptional<T>>
    where T : class
{
    private readonly T? _value;

    private EqualityIgnoredOptional(T? value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the underlying value. This is not used for equality comparison, but can be accessed if needed.
    /// </summary>
    public T? Value => _value;

    /// <inheritdoc/>
    public bool Equals(EqualityIgnoredOptional<T> other)
        => true;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is EqualityIgnoredOptional<T>;

    /// <inheritdoc/>
    public override int GetHashCode()
        => 0;

    /// <inheritdoc/>
    public override string ToString()
        => _value?.ToString() ?? string.Empty;

    public static implicit operator EqualityIgnoredOptional<T>(T? value)
        => new(value);

    public static implicit operator T?(EqualityIgnoredOptional<T> wrapper)
        => wrapper._value;
}
