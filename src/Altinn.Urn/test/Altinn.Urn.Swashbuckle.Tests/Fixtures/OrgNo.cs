﻿using Altinn.Swashbuckle.Examples;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.Swashbuckle.Tests.Fixtures;

public record OrgNo
    : IParsable<OrgNo>
    , ISpanParsable<OrgNo>
    , IFormattable
    , ISpanFormattable
    , IExampleDataProvider<OrgNo>
{
    private static readonly SearchValues<char> NUMBERS = SearchValues.Create(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);

    private readonly string _value;

    private OrgNo(string value)
    {
        _value = value;
    }

    public static IEnumerable<OrgNo>? GetExamples(ExampleDataOptions options)
        => [new("123456789"), new("987654321")];

    public static OrgNo Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : throw new FormatException("Invalid SSN");

    public static OrgNo Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : throw new FormatException("Invalid SSN");

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out OrgNo result)
        => TryParse(s.AsSpan(), provider, s, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out OrgNo result)
        => TryParse(s, provider, null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, string? original, [MaybeNullWhen(false)] out OrgNo result)
    {
        if (s.Length != 9)
        {
            result = null;
            return false;
        }

        if (s.ContainsAnyExcept(NUMBERS))
        {
            result = null;
            return false;
        }

        result = new OrgNo(original ?? new string(s));
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
        => _value;

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length < _value.Length)
        {
            charsWritten = 0;
            return false;
        }

        _value.AsSpan().CopyTo(destination);
        charsWritten = _value.Length;
        return true;
    }
}
