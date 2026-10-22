using System.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;

namespace Altinn.Authorization.CommandLine.Arguments;

internal sealed class ArgumentEnumConfigureArgument
    : IConfigureArgument
    , IConfigureOption
{
    public void Configure<T>(Argument<T> argument)
    {
        if (!typeof(T).IsEnum)
        {
            return;
        }

        if (!typeof(T).IsDefined(typeof(ArgumentEnumAttribute), inherit: false))
        {
            return;
        }

        if (argument.CustomParser is not null)
        {
            return;
        }

        var model = ArgumentEnumModel<T>.Instance;
        argument.HelpCustomization.Argument = model.HelpCustomization;

        if (argument.HasDefaultValue && argument.HelpCustomization.GetDefaultValue is null)
        {
            argument.HelpCustomization.GetDefaultValue = context => GetDefaultValue<T>(context, argument.Name);
        }
    }

    public void Configure<T>(Option<T> option)
    {
        if (!typeof(T).IsEnum)
        {
            return;
        }

        if (!typeof(T).IsDefined(typeof(ArgumentEnumAttribute), inherit: false))
        {
            return;
        }

        if (option.CustomParser is not null)
        {
            return;
        }

        var model = ArgumentEnumModel<T>.Instance;
        option.HelpCustomization.Argument = model.HelpCustomization;

        if (option.HasDefaultValue && option.HelpCustomization.GetDefaultValue is null)
        {
            option.HelpCustomization.GetDefaultValue = context => GetDefaultValue<T>(context, option.Name);
        }
    }

    private static IEnumerable<TextSegment> GetDefaultValue<T>(HelpDisplayArgumentContext context, string name)
    {
        if (context.DefaultValueProvider is not DefaultValueProvider<T> defaultValueProvider)
        {
            return [];
        }

        if (!defaultValueProvider.TryProvideValue(new(name), out var value, out _))
        {
            return [];
        }

        return [new(ArgumentEnumModel<T>.Instance.GetDisplayName(value), context.DefaultArgumentColor)];
    }

}
