namespace Altinn.Urn;

/// <summary>Provides functionality to format the string representation of an object into a URN.</summary>
public interface IUrnFormattable
{
    /// <summary>Tries to format the value of the current instance into the provided span of characters, which is part of an URN being built.</summary>
    /// <param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, the number of characters that were written in <paramref name="destination"/>.</param>
    /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Should provide the same formatting as <see cref="UrnFormat"/>.
    /// TryFormat should return false only if there is not enough space in the destination buffer. Any other failures should throw an exception.
    /// </remarks>
    bool TryUrnFormat(Span<char> destination, out int charsWritten);

    /// <summary>
    /// Formats the value of the current instance as a string.
    /// </summary>
    /// <returns>The formatted string.</returns>
    string UrnFormat();
}
