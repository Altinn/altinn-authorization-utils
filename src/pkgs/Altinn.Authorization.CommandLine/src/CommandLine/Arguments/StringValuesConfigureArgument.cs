using System.CommandLine;
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
    }

    public void Configure<T>(Option<T> option)
    {
        if (option is not Option<StringValues> values || values.CustomParser is not null)
        {
            return;
        }

        values.Arity = ArgumentArity.OneOrMore;
    }

}
