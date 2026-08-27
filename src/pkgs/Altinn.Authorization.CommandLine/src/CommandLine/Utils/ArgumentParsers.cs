using System.CommandLine.Parsing;

namespace Altinn.Authorization.CommandLine.Utils;

/// <summary>
/// Defines a static class that provides methods for creating custom argument parsers for command line options.
/// </summary>
public static class ArgumentParsers
{
    /// <summary>
    /// Creates an argument parser for a type that implements <see cref="IParsable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <returns>A function that parses an argument result into the specified type.</returns>
    public static Func<ArgumentResult, T> TryParse<T>()
        where T : IParsable<T>
    {
        return result =>
        {
            if (result.Tokens.Count == 0)
            {
                result.AddError("No value specified.");
                return default!;
            }

            if (result.Tokens.Count > 1)
            {
                result.AddError("Multiple values specified.");
                return default!;
            }

            var token = result.Tokens[0].Value;
            if (!T.TryParse(token, null, out var value))
            {
                result.AddError($"Invalid value: {token}");
                return default!;
            }

            return value;
        };
    }
}
