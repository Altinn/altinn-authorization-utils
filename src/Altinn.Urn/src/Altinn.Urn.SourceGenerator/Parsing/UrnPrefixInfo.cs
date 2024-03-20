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

internal readonly record struct TryParseMode
{
    private readonly bool _existing;

    public TryParseMode(bool existing)
    {
        _existing = existing;
    }

    public bool Existing => _existing;
    public bool Generate => !Existing;
}

internal readonly record struct FormatMode
{
    private const byte EXISTING_MASK = 1 << 0;
    private const byte FORMAT_ONLY_MASK = 1 << 1;

    // first bit = existing, second bit = format-only
    private readonly byte _value;

    public FormatMode(bool existing, bool formatOnly)
    {
        _value = (byte)((existing ? EXISTING_MASK : 0) | (formatOnly ? FORMAT_ONLY_MASK : 0));
    }

    public bool Existing => (_value & EXISTING_MASK) != 0;

    public bool Generate => !Existing;

    public bool FormatOnly => (_value & FORMAT_ONLY_MASK) != 0;

    public bool TryFormatSupport => !FormatOnly;
}
