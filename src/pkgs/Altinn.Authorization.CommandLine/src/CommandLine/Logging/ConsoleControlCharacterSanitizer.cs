using System.Buffers;
using System.Text;

namespace Altinn.Authorization.CommandLine.Logging;

internal static class ConsoleControlCharacterSanitizer
{
    private static readonly SearchValues<char> _charsToEscape = CreateSearchValues();

    public static string? Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        ReadOnlySpan<char> remaining = value.AsSpan();
        int firstEscapedCharacterIndex = IndexOfFirstCharToEscape(remaining);
        if (firstEscapedCharacterIndex < 0)
        {
            return value;
        }

        var sanitized = new StringBuilder();
        sanitized.Append(remaining.Slice(0, firstEscapedCharacterIndex));
        remaining = remaining.Slice(firstEscapedCharacterIndex);

        while (true)
        {
            // remaining[0] is always a character that must be escaped.
            AppendEscaped(ref sanitized, remaining[0]);
            remaining = remaining.Slice(1);

            int next = IndexOfFirstCharToEscape(remaining);
            if (next < 0)
            {
                sanitized.Append(remaining);
                break;
            }

            sanitized.Append(remaining.Slice(0, next));
            remaining = remaining.Slice(next);
        }

        return sanitized.ToString();
    }

    private static void AppendEscaped(ref StringBuilder builder, char c)
    {
        builder.EnsureCapacity(6);
        builder.Append('\\');
        builder.Append('u');
        builder.Append(ToHexChar(c >> 12));
        builder.Append(ToHexChar(c >> 8));
        builder.Append(ToHexChar(c >> 4));
        builder.Append(ToHexChar(c));
    }

    private static char ToHexChar(int nibble)
    {
        nibble &= 0xF;
        return (char)(nibble < 10 ? '0' + nibble : 'A' + nibble - 10);
    }

    private static int IndexOfFirstCharToEscape(ReadOnlySpan<char> value)
    {
        return value.IndexOfAny(_charsToEscape);
    }

    private static SearchValues<char> CreateSearchValues()
    {
        // ShouldEscape only returns true for characters in the C0/DEL/C1 range (<= U+009F),
        // so there is no need to scan the rest of the BMP when building the set.
        var chars = new List<char>();
        for (int c = 0; c <= 0x9F; c++)
        {
            if (ShouldEscape((char)c))
            {
                chars.Add((char)c);
            }
        }

        return SearchValues.Create(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(chars));
    }

    private static bool ShouldEscape(char c)
    {
        // Escape the control characters that can drive terminal escape sequences when written to a
        // console: the C0 range (U+0000-U+001F), DEL (U+007F) and the C1 range (U+0080-U+009F). These
        // are the same ranges sanitized by systemd and OpenSSH for terminal output. \t, \n and \r are
        // control characters but are preserved for log formatting.
        if (c is '\t' or '\n' or '\r')
        {
            return false;
        }

        return c <= '\u001F' || (c >= '\u007F' && c <= '\u009F');
    }
}
