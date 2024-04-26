using Altinn.Swashbuckle.Examples;
using Altinn.Swashbuckle.Filters;
using Altinn.Urn.Sample.Api.Json;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Sample.Api.Models;

[SwaggerString(Pattern = @"^\d{11}$")]
[JsonConverter(typeof(StringParsableJsonConverter))]
public record PersonIdentifier
    : IParsable<PersonIdentifier>
    , ISpanParsable<PersonIdentifier>
    , IFormattable
    , ISpanFormattable
{
    private static readonly SearchValues<char> NUMBERS = SearchValues.Create(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);

    private readonly string _value;

    private PersonIdentifier(string value)
    {
        _value = value;
    }

    public string Value => _value;

    public static PersonIdentifier Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"Invalid {nameof(PersonIdentifier)}");

    public static PersonIdentifier Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"Invalid {nameof(PersonIdentifier)}");

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PersonIdentifier result)
        => TryParse(s.AsSpan(), provider, s, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out PersonIdentifier result)
        => TryParse(s, provider, null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, string? original, [MaybeNullWhen(false)] out PersonIdentifier result)
    {
        if (s.Length != 11)
        {
            result = null;
            return false;
        }

        if (s.ContainsAnyExcept(NUMBERS))
        {
            result = null;
            return false;
        }

        result = new PersonIdentifier(original ?? new string(s));
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

internal class PersonIdentifierExampleDataProvider
    : ExampleDataProvider<PersonIdentifier>
{
    public override IEnumerable<PersonIdentifier>? GetExamples(ExampleDataOptions options)
    {
        yield return PersonIdentifier.Parse("12345678901", null);
        yield return PersonIdentifier.Parse("23456789012", null);
    }
}
