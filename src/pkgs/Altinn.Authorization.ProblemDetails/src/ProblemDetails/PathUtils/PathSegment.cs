namespace Altinn.Authorization.ProblemDetails.PathUtils;

/// <summary>
/// Represents a segment of a path, which can be either a property name or an array index.
/// </summary>
public readonly ref struct PathSegment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathSegment"/> struct with the specified value and type.
    /// </summary>
    /// <param name="value">The value of the path segment.</param>
    /// <param name="type">The type of the path segment.</param>
    /// <param name="remainder">The remaining path after this segment.</param>
    public PathSegment(ReadOnlySpan<char> value, PathSegmentType type, ReadOnlySpan<char> remainder)
    {
        Value = value;
        Type = type;
        Remainder = remainder;
    }

    /// <summary>
    /// Gets the value of the path segment.
    /// </summary>
    public readonly ReadOnlySpan<char> Value { get; }

    /// <summary>
    /// Gets the type of the path segment, indicating whether it is a property name or an array index.
    /// </summary>
    public readonly PathSegmentType Type { get; }

    /// <summary>
    /// Gets the remaining path after this segment.
    /// </summary>
    public readonly ReadOnlySpan<char> Remainder { get; }

    /// <summary>
    /// Defines an implicit conversion from <see cref="PathSegment"/> to <see cref="ReadOnlySpan{Char}"/>.
    /// </summary>
    /// <param name="segment">The <see cref="PathSegment"/> to convert.</param>
    public static implicit operator ReadOnlySpan<char>(PathSegment segment)
        => segment.Value;
}
