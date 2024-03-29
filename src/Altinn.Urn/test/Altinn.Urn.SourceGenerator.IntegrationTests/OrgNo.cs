﻿using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.IntegrationTests;

public record OrgNo
    : IParsable<OrgNo>
    , ISpanParsable<OrgNo>
    , IFormattable
    // purposefully not implementing ISpanFormattable to test the urn generator
{
    private static readonly SearchValues<char> NUMBERS = SearchValues.Create(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);

    private readonly string _value;

    private OrgNo(string value)
    {
        _value = value;
    }

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
}
