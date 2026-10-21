using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Parsing;

internal static class ValueParserInput
{
    extension(ValueParsingContext context)
    {
        public bool TryGetSingleValue(out string text, [NotNullWhen(false)] out string? error)
        {
            if (context.Values.Count != 1)
            {
                text = string.Empty;
                error = context.Values.Count == 0 ? "No value specified." : "Multiple values specified.";
                return false;
            }

            text = context.Values[0];
            error = null;
            return true;
        }
    }
}
