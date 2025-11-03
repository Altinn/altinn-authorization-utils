using Altinn.Urn.SourceGenerator.Utils;

namespace Altinn.Urn.SourceGenerator.Parsing;

internal readonly record struct UrnPrefixInfo
{
    public required TryParseMode TryParseMode { get; init; }

    public required FormatMode FormatMode { get; init; }

    public required EquitableArray<string> Prefixes { get; init; }

    public required int CanonicalPrefixIndex { get; init; }

    public required string Name { get; init; }

    public required string ValueType { get; init; }

    public required bool ValueTypeIsValueType { get; init; }
}

// Priority list:
// 1. TryParseXXX method defined on the URN type itself
// 2. IUrnParsable implemented by the value type
// 3. ISpanParsable implemented by the value type
// 4. IParsable implemented by the value type
internal readonly record struct TryParseMode
{
    public static TryParseMode None => new(Mode.None);
    public static TryParseMode ExplicitlyDefined => new(Mode.ExplicitlyDefined);
    public static TryParseMode UrnParsable => new(Mode.UrnParsable);
    public static TryParseMode SpanParsable => new(Mode.SpanParsable);
    public static TryParseMode Parsable => new(Mode.Parsable);

    private readonly Mode _value;

    private TryParseMode(Mode mode)
    {
        _value = mode;
    }

    public bool IsValid => _value != Mode.None;

    public bool Generate => _value > Mode.ExplicitlyDefined;

    public bool UseUrnParsable => _value == Mode.UrnParsable;

    public bool UseSpanParsable => _value == Mode.SpanParsable;

    public bool UseParsable => _value == Mode.Parsable;

    private enum Mode
        : byte
    {
        None,
        ExplicitlyDefined,
        UrnParsable,
        SpanParsable,
        Parsable,
    }
}

// Priority list:
// 1. Existing TryFormatXXX & FormatXXX defined on the URN type itself
// 2. Existing FormatXXX defined on the URN type itself
// 3. IUrnFormattable implemented by the value type
// 4. ISpanFormattable implemented by the value type
// 5. IFormattable implemented by the value type
internal readonly record struct FormatMode
{
    public static FormatMode None => new(Mode.None);
    public static FormatMode ExplicitlyDefinedBoth => new(Mode.ExplicitlyDefinedBoth);
    public static FormatMode ExplicitlyDefinedFormatOnly => new(Mode.ExplicitlyDefinedFormatOnly);
    public static FormatMode UrnFormattable => new(Mode.UrnFormattable);
    public static FormatMode SpanFormattable => new(Mode.SpanFormattable);
    public static FormatMode Formattable => new(Mode.Formattable);

    private readonly Mode _value;

    private FormatMode(Mode mode)
    {
        _value = mode;
    }

    public bool IsValid => _value != Mode.None;

    public bool GenerateFormat => _value > Mode.ExplicitlyDefinedFormatOnly;

    public bool GenerateTryFormat => _value > Mode.ExplicitlyDefinedBoth;

    public bool TryFormatUsingFormat => _value is Mode.ExplicitlyDefinedFormatOnly or Mode.Formattable;

    public bool UseUrnFormattable => _value == Mode.UrnFormattable;

    public bool UseSpanFormattable => _value == Mode.SpanFormattable;

    private enum Mode
        : byte
    {
        None,
        ExplicitlyDefinedBoth,
        ExplicitlyDefinedFormatOnly,
        UrnFormattable,
        SpanFormattable,
        Formattable,
    }
}
