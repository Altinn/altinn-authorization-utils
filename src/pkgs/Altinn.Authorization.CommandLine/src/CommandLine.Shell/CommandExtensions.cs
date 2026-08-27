using Altinn.Authorization.CommandLine.Results;

namespace Altinn.Authorization.CommandLine.Shell;

/// <summary>
/// Extensions for the <see cref="Command"/> class.
/// </summary>
public static class CommandExtensions
{
    extension(Command command)
    {
        /// <summary>
        /// Creates a new <see cref="ICommandResult"/> that executes the command.
        /// </summary>
        /// <param name="echo">Whether to echo the command to the console before executing it.</param>
        /// <returns>The <see cref="ICommandResult"/> that executes the command.</returns>
        public ICommandResult ToResult(bool echo = true)
        {
            return new CommandResult(command, echo);
        }
    }
}
