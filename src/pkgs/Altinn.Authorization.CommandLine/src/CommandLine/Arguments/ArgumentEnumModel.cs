using System.Collections.Frozen;
using System.Reflection;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;

namespace Altinn.Authorization.CommandLine.Arguments;

internal sealed class ArgumentEnumModel<T>
{
    private static ArgumentEnumModel<T>? _instance;
    public static ArgumentEnumModel<T> Instance => _instance ??= CreateModel();

#pragma warning disable CS8714 // T is verified to be a non-null enum by the caller.
    private readonly FrozenDictionary<T, string> _display;

    private ArgumentEnumModel(
        FrozenDictionary<string, T> values,
        HelpDisplayArgumentCustomization helpCustomization,
        FrozenDictionary<T, string> display)
    {
        Values = values;
        HelpCustomization = helpCustomization;
        _display = display;
    }
#pragma warning restore CS8714

    public FrozenDictionary<string, T> Values { get; }
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

        return new(frozenByName, HelpDisplayArgumentCustomization.Create(byName.Keys), frozenByValue);
    }
}
