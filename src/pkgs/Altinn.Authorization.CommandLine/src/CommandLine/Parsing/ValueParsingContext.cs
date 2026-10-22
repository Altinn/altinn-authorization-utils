using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Parsing;

/// <summary>Input values and delegation through the configured parsers.</summary>
public sealed class ValueParsingContext
{
    private readonly ValueParser _parser;
    private readonly Type? _valueType;
    private readonly int _nextParser;

    internal ValueParsingContext(
        IReadOnlyList<string> values,
        ValueParser parser,
        ParsingOptions options,
        Type? valueType = null,
        int nextParser = 0)
    {
        Guard.IsNotNull(values);
        Values = values;
        _parser = parser;
        Options = options;
        _valueType = valueType;
        _nextParser = nextParser;
    }

    internal ParsingOptions Options { get; }

    /// <summary>Gets the input values. An environment variable supplies one value.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Delegates parsing. The same type continues with the next parser; an inner type starts from the first parser.</summary>
    public bool TryParse<T>(out T value, [NotNullWhen(false)] out string? error)
        => _parser.TryParse(this, out value, out error);

    /// <summary>Delegates parsing of replacement or inner values, preserving the options snapshot.</summary>
    public bool TryParse<T>(IReadOnlyList<string> values, out T value, [NotNullWhen(false)] out string? error)
        => _parser.TryParse(new ValueParsingContext(values, _parser, Options, _valueType, _nextParser), out value, out error);

    internal int GetStartIndex(Type valueType) => valueType == _valueType ? _nextParser : 0;

    internal ValueParsingContext ForParser(Type valueType, int nextParser)
        => new(Values, _parser, Options, valueType, nextParser);
}
