using System.CommandLine;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Supports formatting command line help.
/// </summary>
public class HelpContext
{
    /// <param name="helpBuilder">The current help builder.</param>
    /// <param name="command">The command for which help is being formatted.</param>
    /// <param name="profile">The console profile.</param>
    public HelpContext(
        IHelpBuilder helpBuilder,
        Command command,
        Profile profile)
    {
        HelpBuilder = helpBuilder ?? throw new ArgumentNullException(nameof(helpBuilder));
        Command = command ?? throw new ArgumentNullException(nameof(command));
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    /// <summary>
    /// The help builder for the current operation.
    /// </summary>
    public IHelpBuilder HelpBuilder { get; }

    /// <summary>
    /// The command for which help is being formatted.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// A text writer to write output to.
    /// </summary>
    public Profile Profile { get; }
}
