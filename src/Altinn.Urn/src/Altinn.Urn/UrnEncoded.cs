using Microsoft.VisualBasic;
using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Altinn.Urn;

/// <summary>
/// Represents a URN-encoded string value. This is mostly the same as URL-encoding, but does not allow for ':'.
/// </summary>
public sealed class UrnEncoded
    : IEquatable<UrnEncoded>
    , IEquatable<string>
    , ISpanFormattable
    , IUrnFormattable
    , IUrnParsable<UrnEncoded>
{
    /// <summary>
    /// Creates a new instance of the <see cref="UrnEncoded"/> class from the specified string value.
    /// </summary>
    /// <param name="value">The string to encode as a URN component. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="UrnEncoded"/> instance representing the encoded form of the specified value.</returns>
    public static UrnEncoded Create(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var encoded = EscapeString(value);
        return new(value, encoded);
    }

    /// <summary>
    /// Attempts to unescape a URN-encoded value from the specified character span.
    /// </summary>
    /// <param name="s">The read-only span of characters containing the URN-encoded value to unescape.</param>
    /// <param name="result">When this method returns, contains the unescaped value as a <see cref="UrnEncoded"/> instance if the operation succeeds;
    /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the value was successfully unescaped; otherwise, <see langword="false"/>.</returns>
    public static bool TryUnescape(ReadOnlySpan<char> s, [MaybeNullWhen(returnValue: false)] out UrnEncoded result)
    {
        if (TryUnescapeValue(s, out var unescaped))
        {
            result = Create(unescaped);
            return true;
        }

        result = null;
        return false;
    }

    private readonly string _value;
    private readonly string _encoded;

    private UrnEncoded(string value, string encoded)
    {
        _value = value;
        _encoded = encoded;
    }

    /// <summary>
    /// Gets the unescaped string value.
    /// </summary>
    public string Value => _value;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj switch
        {
            UrnEncoded other => Equals(other),
            string str => Equals(str),
            _ => false,
        };

    /// <inheritdoc/>
    public bool Equals(UrnEncoded? other)
        => other is not null && _value == other._value;

    /// <inheritdoc/>
    public bool Equals(string? other)
        => other is not null && _value == other;

    /// <inheritdoc/>
    public override int GetHashCode()
        => _value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString()
        => _value;

    /// <inheritdoc/>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        => format == "u"
        ? _encoded
        : _value;

    /// <inheritdoc/>
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var value = format == "u"
            ? _encoded
            : _value;

        var result = value.TryCopyTo(destination);
        charsWritten = result ? value.Length : 0;
        return result;
    }

    /// <inheritdoc/>
    string IUrnFormattable.UrnFormat()
        => _encoded;

    /// <inheritdoc/>
    bool IUrnFormattable.TryUrnFormat(Span<char> destination, out int charsWritten)
    {
        var value = _encoded;

        var result = value.TryCopyTo(destination);
        charsWritten = result ? value.Length : 0;
        return result;
    }

    /// <inheritdoc/>
    static bool IUrnParsable<UrnEncoded>.TryParseUrnValue(ReadOnlySpan<char> s, [MaybeNullWhen(returnValue: false)] out UrnEncoded result)
        => TryUnescape(s, out result);

    private const int StackallocThreshold = 512;

    /// <summary>All ASCII letters and digits, as well as the RFC3986 reserved and unreserved marks except questionmark, colon, plus and hash.</summary>
    private static readonly SearchValues<char> UnreservedReservedExceptQuestionMarkHash
        = SearchValues.Create("!$&'()*,-./0123456789;=@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]_abcdefghijklmnopqrstuvwxyz~");

    private static readonly SearchValues<char> UnescapeChars = SearchValues.Create("%+");

    private static readonly FrozenDictionary<Rune, string> WellKnownSafeRunes = CreateSafeRuneDict();

    private static FrozenDictionary<Rune, string> CreateSafeRuneDict()
        => new List<string> { "Æ", "Ø", "Å", "æ", "ø", "å" }
            .Select(s => KeyValuePair.Create(Rune.GetRuneAt(s, 0), s))
            .ToFrozenDictionary();

    private static bool TryUnescapeValue(ReadOnlySpan<char> s, [MaybeNullWhen(returnValue: false)] out string result)
    {
        var indexOfFirstEscaped = s.IndexOfAny(UnescapeChars);
        if (indexOfFirstEscaped < 0) 
        {
            // No escaped characters, return as-is.
            result = new string(s);
            return true;
        }

        // Otherwise, create a ValueStringBuilder to store the unescaped data into,
        // escape the rest, and concat the result with the characters we skipped above.
        var vsb = new ValueStringBuilder(stackalloc char[StackallocThreshold]);

        // We may throw for very large inputs (when growing the ValueStringBuilder).
        vsb.EnsureCapacity(s.Length);

        UnescapeStringToBuilder(s[indexOfFirstEscaped..], ref vsb);

        result = string.Concat(s[..indexOfFirstEscaped], vsb.AsSpan());
        vsb.Dispose();
        return true;
    }

    private static string EscapeString(string stringToEscape)
        => EscapeString(stringToEscape, stringToEscape);

    private static string EscapeString(ReadOnlySpan<char> charsToEscape, string? backingString)
    {
        Debug.Assert(backingString is null || backingString.Length == charsToEscape.Length);

        var indexOfFirstToEscape = charsToEscape.IndexOfAnyExcept(UnreservedReservedExceptQuestionMarkHash);
        if (indexOfFirstToEscape < 0)
        {
            // Nothing to escape, just return the original value.
            return backingString ?? new string(charsToEscape);
        }

        // Otherwise, create a ValueStringBuilder to store the escaped data into,
        // escape the rest, and concat the result with the characters we skipped above.
        var vsb = new ValueStringBuilder(stackalloc char[StackallocThreshold]);

        // We may throw for very large inputs (when growing the ValueStringBuilder).
        vsb.EnsureCapacity(charsToEscape.Length);

        EscapeStringToBuilder(charsToEscape[indexOfFirstToEscape..], ref vsb);

        string result = string.Concat(charsToEscape[..indexOfFirstToEscape], vsb.AsSpan());
        vsb.Dispose();
        return result;
    }

#if NET9_0_OR_GREATER
    private static void UnescapeStringToBuilder(
        scoped ReadOnlySpan<char> stringToUnescape,
        ref ValueStringBuilder vsb)
    {
        int written;

        var firstPlusIndex = stringToUnescape.IndexOf('+');
        if (firstPlusIndex < 0)
        {
            if (!Uri.TryUnescapeDataString(stringToUnescape, vsb.WriteBuffer, out written))
            {
                throw new InvalidOperationException("Failed to unescape string");
            }

            vsb.Advance(written);
            return;
        }

        var scratch = new ValueStringBuilder(stackalloc char[StackallocThreshold]);
        scratch.Append(stringToUnescape);
        scratch.RawChars.Slice(0, scratch.Length).Replace('+', ' ');

        if (!Uri.TryUnescapeDataString(scratch.AsSpan(), vsb.WriteBuffer, out written))
        {
            throw new InvalidOperationException("Failed to unescape string");
        }

        scratch.Dispose();
        vsb.Advance(written);
    }
#else
    private static void UnescapeStringToBuilder(
        scoped ReadOnlySpan<char> stringToUnescape,
        ref ValueStringBuilder vsb)
    {
        var unescaped = Uri.UnescapeDataString(new string(stringToUnescape).Replace('+', ' '));
        vsb.Append(unescaped);
    }
#endif

    // Copied from System.UriHelper and removed unused arguments
    private static void EscapeStringToBuilder(
        scoped ReadOnlySpan<char> stringToEscape,
        ref ValueStringBuilder vsb)
    {
        Debug.Assert(!stringToEscape.IsEmpty && !UnreservedReservedExceptQuestionMarkHash.Contains(stringToEscape[0]));

        // Allocate enough stack space to hold any Rune's UTF8 encoding.
        Span<byte> utf8Bytes = stackalloc byte[4];

        while (!stringToEscape.IsEmpty)
        {
            char c = stringToEscape[0];

            if (c == ' ')
            {
                vsb.Append('+');
                stringToEscape = stringToEscape.Slice(1);
                continue;
            }

            if (!char.IsAscii(c))
            {
                if (Rune.DecodeFromUtf16(stringToEscape, out Rune r, out int charsConsumed) != OperationStatus.Done)
                {
                    r = Rune.ReplacementChar;
                }

                Debug.Assert(stringToEscape.EnumerateRunes() is { } e && e.MoveNext() && e.Current == r);
                Debug.Assert(charsConsumed is 1 or 2);

                stringToEscape = stringToEscape.Slice(charsConsumed);

                if (WellKnownSafeRunes.TryGetValue(r, out var encodedRune))
                {
                    vsb.Append(encodedRune);
                    continue;
                }

                // The rune is non-ASCII, so encode it as UTF8, and escape each UTF8 byte.
                r.TryEncodeToUtf8(utf8Bytes, out int bytesWritten);
                foreach (byte b in utf8Bytes.Slice(0, bytesWritten))
                {
                    PercentEncodeByte(b, ref vsb);
                }

                continue;
            }

            if (!UnreservedReservedExceptQuestionMarkHash.Contains(c))
            {
                PercentEncodeByte((byte)c, ref vsb);
                stringToEscape = stringToEscape.Slice(1);
                continue;
            }

            // We have a character we don't want to escape. It's likely there are more, do a vectorized search.
            int charsToCopy = stringToEscape.IndexOfAnyExcept(UnreservedReservedExceptQuestionMarkHash);
            if (charsToCopy < 0)
            {
                charsToCopy = stringToEscape.Length;
            }
            Debug.Assert(charsToCopy > 0);

            vsb.Append(stringToEscape[..charsToCopy]);
            stringToEscape = stringToEscape[charsToCopy..];
        }
    }

    private static void PercentEncodeByte(byte value, ref ValueStringBuilder to)
    {
        uint difference = (((uint)value & 0xF0U) << 4) + ((uint)value & 0x0FU) - 0x8989U;
        uint packedResult = ((((uint)(-(int)difference) & 0x7070U) >> 4) + difference + 0xB9B9U);

        var span = to.AppendSpan(3);
        span[0] = '%';
        span[1] = (char)(packedResult >> 8);
        span[2] = (char)(packedResult & 0xFF);
    }
}
