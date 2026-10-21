using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.CommandLine.Parsing;

/// <summary>Parses values using the current options and built-in conversion.</summary>
public sealed class ValueParser(IOptionsMonitor<ParsingOptions> options)
{
    private static readonly DefaultValueParser _defaultParser = new();

    /// <summary>Parses input values using the configured parsers.</summary>
    public bool TryParse<T>(IReadOnlyList<string> values, out T value, [NotNullWhen(false)] out string? error)
        => TryParse(new ValueParsingContext(values, this, options.CurrentValue), out value, out error);

    internal bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
    {
        var parsers = context.Options.AdditionalParsers;
        var start = context.GetStartIndex(typeof(T));

        for (var i = start; i < parsers.Count; i++)
        {
            if (parsers[i].CanParse(typeof(T)))
            {
                return parsers[i].TryParse(context.ForParser(typeof(T), i + 1), out value, out error);
            }
        }

        if (start <= parsers.Count)
        {
            return _defaultParser.TryParse(context.ForParser(typeof(T), parsers.Count + 1), out value, out error);
        }

        value = default!;
        error = $"No value parser is registered for '{typeof(T)}'.";
        return false;
    }
}
