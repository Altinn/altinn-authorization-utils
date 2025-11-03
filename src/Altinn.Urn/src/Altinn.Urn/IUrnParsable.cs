using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn;

/// <summary>Defines a mechanism for parsing a span of characters from an URN to a value.</summary>
/// <typeparam name="TSelf">The type that implements this interface.</typeparam>
public interface IUrnParsable<TSelf>
    where TSelf : IUrnParsable<TSelf>
{
    /// <summary>Tries to parse a span of characters into a value.</summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="result">On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
    /// <returns><see langword="true"/> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    static abstract bool TryParseUrnValue(ReadOnlySpan<char> s, [MaybeNullWhen(returnValue: false)] out TSelf result);
}
