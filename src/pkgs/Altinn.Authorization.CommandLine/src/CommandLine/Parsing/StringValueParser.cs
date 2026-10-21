using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class StringValueParser
{
    public bool TryParse(ValueParsingContext context, out string value, [NotNullWhen(false)] out string? error)
        => context.TryGetSingleValue(out value, out error);
}
