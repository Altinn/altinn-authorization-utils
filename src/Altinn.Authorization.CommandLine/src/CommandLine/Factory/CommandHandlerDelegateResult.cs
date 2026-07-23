using System.CommandLine;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Represents the result of creating a <see cref="CommandHandlerDelegate"/> using the <see cref="CommandHandlerDelegateFactory"/>.
/// </summary>
public sealed class CommandHandlerDelegateResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandlerDelegateResult"/> class.
    /// </summary>
    /// <param name="commandHandlerDelegate">See <see cref="Delegate"/>.</param>
    /// <param name="options">See <see cref="Options"/>.</param>
    /// <param name="arguments">See <see cref="Arguments"/>.</param>
    public CommandHandlerDelegateResult(
        CommandHandlerDelegate commandHandlerDelegate,
        IReadOnlyList<Option> options,
        IReadOnlyList<Argument> arguments)
    {
        Delegate = commandHandlerDelegate;
        Options = options;
        Arguments = arguments;
    }

    /// <summary>
    /// Gets the <see cref="CommandHandlerDelegate"/> that was created by the factory.
    /// </summary>
    public CommandHandlerDelegate Delegate { get; }

    /// <summary>
    /// Gets the list of <see cref="Option"/> instances that were created by the factory for the command handler.
    /// </summary>
    public IReadOnlyList<Option> Options { get; }

    /// <summary>
    /// Gets the list of <see cref="Argument"/> instances that were created by the factory for the command handler.
    /// </summary>
    public IReadOnlyList<Argument> Arguments { get; }
}
