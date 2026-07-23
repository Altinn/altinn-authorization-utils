using System.CommandLine;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Commands;

internal sealed class CommandExtensions
{
    private static readonly ConditionalWeakTable<Command, CommandExtensions> _extensions = new();

    internal static CommandExtensions For(Command command)
    {
        Guard.IsNotNull(command);

        return _extensions.GetOrAdd(command, static _ => new());
    }

    private readonly List<object> _metadat = new();

    public List<object> Metadata
        => _metadat;
}
