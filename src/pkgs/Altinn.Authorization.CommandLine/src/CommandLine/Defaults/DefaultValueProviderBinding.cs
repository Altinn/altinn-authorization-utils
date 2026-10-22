using System.CommandLine;
using System.CommandLine.Parsing;
using Altinn.Authorization.CommandLine.Help;

namespace Altinn.Authorization.CommandLine;

internal static class DefaultValueProviderBinding
{
    public static void Apply<T>(Option<T> option, DefaultValueProvider<T>? provider)
    {
        if (provider is null)
        {
            return;
        }

        option.DefaultValueFactory = CreateFactory(provider, option.Name);
        option.HelpCustomization.DefaultValueProvider = provider;
    }

    public static void Apply<T>(Argument<T> argument, DefaultValueProvider<T>? provider)
    {
        if (provider is null)
        {
            return;
        }

        argument.DefaultValueFactory = CreateFactory(provider, argument.Name);
        argument.HelpCustomization.DefaultValueProvider = provider;
    }

    private static Func<ArgumentResult, T> CreateFactory<T>(DefaultValueProvider<T> provider, string name)
        => result =>
        {
            if (provider.TryProvideValue(new(name), out var value, out var error))
            {
                return value;
            }

            result.AddError(error);
            return default!;
        };
}
