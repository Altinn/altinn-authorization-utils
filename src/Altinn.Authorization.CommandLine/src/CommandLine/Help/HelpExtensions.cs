using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Provides API compatibility with upstream System.CommandLine properties
/// not yet present in the referenced package version.
/// </summary>
public static class HelpExtensions
{
    private static readonly ConditionalWeakTable<RootCommand, string> HelpNames = new();
    private static readonly ConditionalWeakTable<Symbol, HelpCustomization> Customizations = new();

    extension(RootCommand root)
    {
        /// <summary>
        /// Gets or sets the name of the command to be used in help text.
        /// </summary>
        public string? HelpName
        {
            get => HelpNames.TryGetValue(root, out var value) ? value : root.Name;
            set
            {
                if (value is null)
                {
                    HelpNames.Remove(root);
                    return;
                }

                HelpNames.AddOrUpdate(root, value);
            }
        }
    }

    extension(CommandResult commandResult)
    {
        /// <summary>
        /// Gets the parent commands of the specified command, optionally including the command itself.
        /// </summary>
        /// <returns>An enumerable of parent commands.</returns>
        internal IEnumerable<Command> InvocationPath()
        {
            for (SymbolResult? current = commandResult; current is not null; current = current.Parent)
            {
                if (current is CommandResult command)
                {
                    yield return command.Command;
                }
            }
        }
    }

    extension(Symbol symbol)
    {
        internal IEnumerable<Symbol> GetParameters()
        {
            switch (symbol)
            {
                case Option option:
                    yield return option;
                    yield break;

                case Command command:
                    foreach (var argument in command.Arguments)
                    {
                        yield return argument;
                    }
                    yield break;

                case Argument argument:
                    yield return argument;
                    yield break;

                default:
                    ThrowHelper.ThrowNotSupportedException();
                    break;
            }
        }

        internal bool TryGetHelpCustomization([NotNullWhen(true)] out HelpCustomization? customization)
        {
            return Customizations.TryGetValue(symbol, out customization);
        }

        /// <summary>
        /// Gets the help customization for the specified symbol, creating a new instance if it does not already exist.
        /// </summary>
        public HelpCustomization HelpCustomization
        {
            get => Customizations.GetOrAdd(symbol, _ => new());
        }
    }

    extension(string value)
    {
        internal (string? Prefix, string Alias) SplitPrefix()
        {
            return value[0] switch
            {
                '/' => ("/", value[1..]),
                '-' when value.Length > 1 && value[1] is '-' => ("--", value[2..]),
                '-' => ("-", value[1..]),
                _ => (null, value)
            };
        }
    }
}
