using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Parsing;

/// <summary>Converts input from command-line tokens or a default source.</summary>
public interface IValueParser
{
    /// <summary>Gets whether this parser handles the specified value type.</summary>
    bool CanParse(Type type);

    /// <summary>Tries to convert input, returning a command parse diagnostic on failure.</summary>
    /// <typeparam name="T">The parsed value type.</typeparam>
    bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error);
}
