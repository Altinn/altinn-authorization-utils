using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Represents a segment of text with an associated style.
/// </summary>
/// <param name="Text">The text of the segment.</param>
/// <param name="Style">The style associated with the segment.</param>
public record struct TextSegment(string Text, Style? Style)
{
    /// <summary>
    /// Creates an implicit conversion from a string to a <see cref="TextSegment"/>.
    /// </summary>
    /// <param name="text">The text to convert to a <see cref="TextSegment"/>.</param>
    public static implicit operator TextSegment(string text)
        => new(text, null);
}
