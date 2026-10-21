using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class ParsableValueParser<T>
    where T : IParsable<T>
{
    public bool TryParse(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
    {
        if (!context.TryGetSingleValue(out var text, out error))
        {
            value = default!;
            return false;
        }

        if (T.TryParse(text, null, out var parsed))
        {
            value = parsed;
            return true;
        }

        value = default!;
        error = $"Invalid value: {text}";
        return false;
    }
}
