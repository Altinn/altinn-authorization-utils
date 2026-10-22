using System.Collections;
using System.CommandLine;
using System.CommandLine.Parsing;
using Altinn.Authorization.CommandLine.Factory;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class ValueParserConfigureArgument(ValueParser parser)
    : IConfigureArgument
    , IConfigureOption
{
    public void Configure<T>(Argument<T> argument)
        => argument.CustomParser ??= CreateParser<T>();

    public void Configure<T>(Option<T> option)
        => option.CustomParser ??= CreateParser<T>();

    private Func<ArgumentResult, T> CreateParser<T>()
        => CreateParser<T>(parser);

    internal static Func<ArgumentResult, T> CreateParser<T>(ValueParser parser)
        => result =>
        {
            if (parser.TryParse<T>(new TokenValues(result.Tokens), out var value, out var error))
            {
                return value;
            }

            result.AddError(error);
            return default!;
        };

    private sealed class TokenValues(IReadOnlyList<Token> tokens)
        : IReadOnlyList<string>
    {
        public int Count => tokens.Count;
        public string this[int index] => tokens[index].Value;
        public IEnumerator<string> GetEnumerator()
        {
            foreach (var token in tokens)
            {
                yield return token.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
