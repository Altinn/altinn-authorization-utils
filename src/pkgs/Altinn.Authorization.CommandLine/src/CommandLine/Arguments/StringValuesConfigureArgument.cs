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

        values.Arity = values.HasDefaultValue ? ArgumentArity.ZeroOrMore : ArgumentArity.OneOrMore;
        values.CustomParser = Parse;
    }

    public void Configure<T>(Option<T> option)
    {
        if (option is not Option<StringValues> values || values.CustomParser is not null)
        {
            return;
        }

        values.Arity = ArgumentArity.OneOrMore;
        values.CustomParser = Parse;
    }

    private static StringValues Parse(ArgumentResult result)
    {
        return result.Tokens.Count switch
        {
            0 => Empty(result),
            1 => SingleToken(result, result.Tokens[0].Value),
            _ => MultipleTokens(result, result.Tokens),
        };

        static void AddValues(List<string> builder, string token)
        {
            builder.AddRange(token.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        static StringValues Empty(ArgumentResult result)
        {
            result.AddError("At least one non-whitespace value is required.");

            return StringValues.Empty;
        }

        static StringValues SingleToken(ArgumentResult result, string value)
        {
            var builder = new List<string>();
            AddValues(builder, value);

            return builder.Count switch
            {
                0 => Empty(result),
                1 => new StringValues(builder[0]),
                _ => new StringValues([.. builder]),
            };
        }

        static StringValues MultipleTokens(ArgumentResult result, IReadOnlyList<Token> tokens)
        {
            var builder = new List<string>();
            foreach (var token in tokens)
            {
                AddValues(builder, token.Value);
            }

            return builder.Count switch
            {
                0 => Empty(result),
                1 => new StringValues(builder[0]),
                _ => new StringValues([.. builder]),
            };
        }
    }
}
