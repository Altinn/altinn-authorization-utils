using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Utils;

namespace Altinn.Authorization.CommandLine.Arguments;

internal sealed class ParsableConfigureArgument
    : IConfigureArgument
    , IConfigureOption
{
    private static readonly MethodInfo _tryParseFactory =
        typeof(ArgumentParsers).GetMethod(nameof(ArgumentParsers.TryParse))!;

    public void Configure<T>(Argument<T> argument)
    {
        if (argument.CustomParser is not null || argument.DefaultValueFactory is not null)
        {
            return;
        }

        var action = GetTryParseAction<T>();
        if (action is null)
        {
            return;
        }

        argument.CustomParser = action;
    }

    public void Configure<T>(Option<T> option)
    {
        if (option.CustomParser is not null || option.DefaultValueFactory is not null)
        {
            return;
        }

        var action = GetTryParseAction<T>();
        if (action is null)
        {
            return;
        }

        option.CustomParser = action;
    }

    private static Func<ArgumentResult, T>? GetTryParseAction<T>()
    {
        var ifaces = typeof(T).GetInterfaces();
        if (ifaces.Any(static i => i.IsGenericType
            && i.GetGenericTypeDefinition() == typeof(IParsable<>)
            && i.GetGenericArguments()[0] == typeof(T)))
        {
            return (Func<ArgumentResult, T>)_tryParseFactory.MakeGenericMethod(typeof(T)).Invoke(null, [])!;
        }

        return null;
    }
}
