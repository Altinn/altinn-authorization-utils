using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class NullableValueParser<T>
    where T : struct
{
    public bool TryParse(ValueParsingContext context, out T? value, [NotNullWhen(false)] out string? error)
    {
        if (context.Values.Count == 0)
        {
            value = null;
            error = null;
            return true;
        }

        if (context.TryParse<T>(out var parsed, out error))
        {
            value = parsed;
            return true;
        }

        value = null;
        return false;
    }
}
