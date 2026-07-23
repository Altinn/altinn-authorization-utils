using System.CommandLine;
using System.CommandLine.Parsing;
using CommunityToolkit.Diagnostics;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Supports formatting command line help.
/// </summary>
public class HelpContext
{
    /// <param name="helpBuilder">The current help builder.</param>
    /// <param name="commandResult">The command result for which help is being formatted.</param>
    /// <param name="profile">The console profile.</param>
    public HelpContext(
        IHelpBuilder helpBuilder,
        CommandResult commandResult,
        Profile profile)
    {
        Guard.IsNotNull(helpBuilder);
        Guard.IsNotNull(commandResult);
        Guard.IsNotNull(profile);

        HelpBuilder = helpBuilder;
        CommandResult = commandResult;
        Profile = profile;
    }

    /// <summary>
    /// The help builder for the current operation.
    /// </summary>
    public IHelpBuilder HelpBuilder { get; }

    /// <summary>
    /// The command result for which help is being formatted.
    /// </summary>
    public CommandResult CommandResult { get; }

    /// <summary>
    /// The command for which help is being formatted.
    /// </summary>
    public Command Command => CommandResult.Command;

    /// <summary>
    /// A text writer to write output to.
    /// </summary>
    public Profile Profile { get; }
}
