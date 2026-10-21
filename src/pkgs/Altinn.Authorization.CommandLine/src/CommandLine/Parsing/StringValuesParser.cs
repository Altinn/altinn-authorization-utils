using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Primitives;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class StringValuesParser
{
    public bool TryParse(ValueParsingContext context, out StringValues value, [NotNullWhen(false)] out string? error)
    {
        var values = new List<string>();
        foreach (var text in context.Values)
        {
            values.AddRange(text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        value = values.Count switch
        {
            0 => StringValues.Empty,
            1 => new StringValues(values[0]),
            _ => new StringValues([.. values]),
        };
        error = values.Count == 0 ? "At least one non-whitespace value is required." : null;
        return error is null;
    }
}
