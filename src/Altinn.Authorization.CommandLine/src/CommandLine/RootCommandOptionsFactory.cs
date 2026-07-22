using System.CommandLine;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.CommandLine;

internal sealed class RootCommandOptionsFactory
    : OptionsFactory<RootCommand>
{
    public RootCommandOptionsFactory(
        IEnumerable<IConfigureOptions<RootCommand>> setups,
        IEnumerable<IPostConfigureOptions<RootCommand>> postConfigures,
        IEnumerable<IValidateOptions<RootCommand>> validations)
        : base(setups, postConfigures, validations)
    {
    }

    protected sealed override RootCommand CreateInstance(string name)
    {
        if (name != Options.DefaultName)
        {
            ThrowHelper.ThrowArgumentException($"The name '{name}' is not supported. Only the default name is supported.", nameof(name));
        }

        return new();
    }
}
