using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Arguments;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class EnumValueParser<T>
{
    private readonly ArgumentEnumModel<T>? _model = typeof(T).IsDefined(typeof(ArgumentEnumAttribute), false)
        ? ArgumentEnumModel<T>.Instance : null;

    public bool TryParse(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
    {
        if (!context.TryGetSingleValue(out var text, out error))
        {
            value = default!;
            return false;
        }

        if (_model is not null)
        {
            if (_model.Values.TryGetValue(text, out value!))
            {
                return true;
            }

            error = $"Argument '{text}' not recognized. Must be one of: {string.Join(", ", _model.Values.Keys)}";
        }
        else
        {
            if (Enum.TryParse(typeof(T), text, ignoreCase: true, out var parsed))
            {
                value = (T)parsed!;
                return true;
            }

            error = $"Invalid value: {text}";
        }

        value = default!;
        return false;
    }
}
