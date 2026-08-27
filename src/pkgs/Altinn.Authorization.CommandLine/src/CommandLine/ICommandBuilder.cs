namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Defines a builder for configuring commands in the command host.
/// </summary>
public interface ICommandBuilder
    : ICommandHandlerBuilder
{
    /// <summary>
    /// Gets a value indicating whether the command being built is the root command of the application.
    /// </summary>
    public bool IsRoot { get; }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the command aliases.
    /// </summary>
    public ICollection<string> Aliases { get; }

    /// <summary>
    /// Gets or sets the <see cref="CommandHandlerDelegate"/> invoked by this command.
    /// </summary>
    public CommandHandlerDelegate? CommandHandler { get; set; }

    /// <summary>
    /// Adds a command to the command host with the specified name, description, and configuration action.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    /// <param name="configure">The configuration action for the command.</param>
    public ICommandConventionBuilder AddCommand(string name, string description, Action<ICommandBuilder> configure);
}
