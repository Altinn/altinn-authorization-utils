using System.CommandLine;
using System.CommandLine.Parsing;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Options;

internal sealed class ConfigureAltinnVerticalKindOptions
    : IConfigureOption
{
    public void Configure<T>(Option<T> option)
    {
        if (option is Option<AltinnVerticalKind> kindOption)
        {
            ConfigureKindOption(kindOption);
        }
        else if (option is Option<AltinnVerticalKindSet> kindSetOption)
        {
            ConfigureKindSetOption(kindSetOption);
        }
    }

    private static void ConfigureKindOption(Option<AltinnVerticalKind> option)
    {
        var valid = AltinnVerticalKindSet.All.Select(static kind => kind.ToString());
        option.Description ??= "The kind of vertical.";
        option.HelpCustomization.Argument = HelpDisplayArgumentCustomization.Create(valid);
    }

    private static void ConfigureKindSetOption(Option<AltinnVerticalKindSet> option)
    {
        if (option.Required)
        {
            option.Arity = ArgumentArity.OneOrMore;
        }
        else
        {
            option.Arity = ArgumentArity.ZeroOrMore;
        }

        var valid = AltinnVerticalKindSet.All.Select(static kind => kind.ToString()).Concat(["all"]);
        option.Description ??= "The kinds of verticals.";
        option.HelpCustomization.Argument = HelpDisplayArgumentCustomization.Create(valid);

        option.CustomParser = (ArgumentResult result) =>
        {
            var kinds = AltinnVerticalKindSet.Empty;

            foreach (var token in result.Tokens)
            {
                var tokenSpan = token.Value.AsSpan();
                foreach (var range in tokenSpan.Split(','))
                {
                    var value = tokenSpan[range].Trim();
                    if (value.IsEmpty)
                    {
                        continue;
                    }

                    if (value is "all")
                    {
                        kinds = AltinnVerticalKindSet.All;
                        continue;
                    }

                    if (!AltinnVerticalKind.TryParse(value, null, out var kind))
                    {
                        result.AddError($"The value '{value}' is not a valid AltinnVerticalKind.");
                        continue;
                    }

                    kinds = kinds.Add(kind);
                }
            }

            return kinds;
        };
    }
}
