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

        if (argument.HasDefaultValue)
        {
            try
            {
                var defaultValue = (T)argument.GetDefaultValue()!;
                var displayName = model.GetDisplayName(defaultValue);
                argument.HelpCustomization.GetDefaultValue = context => [new(displayName, context.DefaultArgumentColor)];
            }
            catch { }
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

        if (option.HasDefaultValue)
        {
            try
            {
                var defaultValue = (T)option.GetDefaultValue()!;
                var displayName = model.GetDisplayName(defaultValue);
                option.HelpCustomization.GetDefaultValue = context => [new(displayName, context.DefaultArgumentColor)];
            }
            catch { }
        }
    }

}
