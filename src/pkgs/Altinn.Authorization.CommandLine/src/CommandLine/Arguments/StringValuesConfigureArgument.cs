using System.CommandLine;
using System.CommandLine.Parsing;
using Altinn.Authorization.CommandLine.Factory;
using Microsoft.Extensions.Primitives;

namespace Altinn.Authorization.CommandLine.Arguments;

internal sealed class StringValuesConfigureArgument
    : IConfigureArgument
    , IConfigureOption
{
    public void Configure<T>(Argument<T> argument)
    {
        if (argument is not Argument<StringValues> values || values.CustomParser is not null)
        {
            return;
        }

        var required = !values.HasDefaultValue;
        values.Arity = required ? ArgumentArity.OneOrMore : ArgumentArity.ZeroOrMore;
        values.CustomParser = result => Parse(result, required);
    }

    public void Configure<T>(Option<T> option)
    {
        if (option is not Option<StringValues> values || values.CustomParser is not null)
        {
            return;
        }

        values.Arity = values.Required ? ArgumentArity.OneOrMore : ArgumentArity.ZeroOrMore;
        values.CustomParser = result => Parse(result, values.Required);
    }

    private static StringValues Parse(ArgumentResult result, bool required)
    {
        return result.Tokens.Count switch
        {
            0 => Empty(result, required),
            1 => SingleToken(result, result.Tokens[0].Value, required),
            _ => MultipleTokens(result, result.Tokens, required),
        };

        static void AddValues(List<string> builder, string token)
        {
            builder.AddRange(token.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        static StringValues Empty(ArgumentResult result, bool required)
        {
            if (required)
            {
                result.AddError("At least one non-whitespace value is required.");
            }

            return StringValues.Empty;
        }

        static StringValues SingleToken(ArgumentResult result, string value, bool required)
        {
            var builder = new List<string>();
            AddValues(builder, value);

            return builder.Count switch
            {
                0 => Empty(result, required),
                1 => new StringValues(builder[0]),
                _ => new StringValues([.. builder]),
            };
        }

        static StringValues MultipleTokens(ArgumentResult result, IReadOnlyList<Token> tokens, bool required)
        {
            var builder = new List<string>();
            foreach (var token in tokens)
            {
                AddValues(builder, token.Value);
            }

            return builder.Count switch
            {
                0 => Empty(result, required),
                1 => new StringValues(builder[0]),
                _ => new StringValues([.. builder]),
            };
        }
    }
}
