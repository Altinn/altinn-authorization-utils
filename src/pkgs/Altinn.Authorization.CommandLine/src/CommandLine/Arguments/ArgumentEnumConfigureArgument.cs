using System.Collections.Frozen;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Spectre.Console;

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
        argument.CustomParser = model.Parser;
        argument.HelpCustomization.Argument = model.HelpCustomization;

        if (argument.HasDefaultValue)
        {
            try
            {
                var defaultValue = (T)argument.GetDefaultValue()!;
                var displayName = model.GetDisplayName(defaultValue);
                argument.HelpCustomization.GetDefaultValue = () => [new(displayName, Color.Cyan)];
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
        option.CustomParser = model.Parser;
        option.HelpCustomization.Argument = model.HelpCustomization;

        if (option.HasDefaultValue)
        {
            try
            {
                var defaultValue = (T)option.GetDefaultValue()!;
                var displayName = model.GetDisplayName(defaultValue);
                option.HelpCustomization.GetDefaultValue = () => [new(displayName, Color.Cyan)];
            }
            catch { }
        }
    }

    private sealed class ArgumentEnumModel<T>
    {
        private static ArgumentEnumModel<T>? _instance;
        public static ArgumentEnumModel<T> Instance => _instance ??= CreateModel();

#pragma warning disable CS8714 // T is verified to be a non-null enum by the caller.
        private readonly FrozenDictionary<T, string> _display;

        private ArgumentEnumModel(
            Func<ArgumentResult, T> parser,
            HelpDisplayArgumentCustomization helpCustomization,
            FrozenDictionary<T, string> display)
        {
            Parser = parser;
            HelpCustomization = helpCustomization;
            _display = display;
        }
#pragma warning restore CS8714

        public Func<ArgumentResult, T> Parser { get; }
        public HelpDisplayArgumentCustomization HelpCustomization { get; }
        public string GetDisplayName(T value)
            => _display.TryGetValue(value, out var name)
            ? name
            : value!.ToString()!;

        private static ArgumentEnumModel<T> CreateModel()
        {
#pragma warning disable CS8714 // T is verified to be a non-null enum by the caller.
            var byName = new Dictionary<string, T>();
            var byValue = new Dictionary<T, string>();
#pragma warning restore CS8714

            foreach (var field in typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var value = (T)field.GetValue(null)!;
                var valueAttribute = field.GetCustomAttribute<ArgumentValuesAttribute>(inherit: false);

                if (valueAttribute is null)
                {
                    throw new InvalidOperationException($"Enum value '{field.Name}' of type '{TypeNameHelper.GetTypeDisplayName(typeof(T))}' is missing the required {nameof(ArgumentValuesAttribute)}");
                }

                foreach (var name in valueAttribute.Names)
                {
                    if (!byName.TryAdd(name, value))
                    {
                        throw new InvalidOperationException($"Enum value '{field.Name}' of type '{TypeNameHelper.GetTypeDisplayName(typeof(T))}' has a duplicate name '{name}'");
                    }
                }

                byValue[value] = valueAttribute.CanonicalName;
            }

#pragma warning disable CS8714 // T is verified to be a non-null enum by the caller.
            var frozenByName = byName.ToFrozenDictionary();
            var frozenByValue = byValue.ToFrozenDictionary();
#pragma warning restore CS8714

            Func<ArgumentResult, T> parser = result =>
            {
                if (result.Tokens.Count == 0)
                {
                    result.AddError($"The argument '{result.Argument.Name}' is required.");
                    return default!;
                }

                if (result.Tokens.Count > 1)
                {
                    result.AddError($"The '{result.Argument.Name}' option can only be specified once.");
                    return default!;
                }

                var token = result.Tokens[0].Value;

                if (!frozenByName.TryGetValue(token, out var parsedValue))
                {
                    result.AddError($"Argument '{token}' not recognized. Must be one of: {string.Join(", ", frozenByName.Keys)}");
                    return default!;
                }

                return parsedValue;
            };

            return new(parser, HelpDisplayArgumentCustomization.Create(byName.Keys), frozenByValue);
        }
    }
}
