using System.CommandLine;
using System.CommandLine.Parsing;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Defines a builder for configuring command handlers in the command host.
/// </summary>
public interface ICommandHandlerBuilder
{
    /// <summary>
    /// Gets the command options.
    /// </summary>
    public IList<Option> Options { get; }

    /// <summary>
    /// Gets the command arguments.
    /// </summary>
    public IList<Argument> Arguments { get; }

    /// <summary>
    /// Gets the command validators.
    /// </summary>
    public IList<Action<CommandResult>> Validators { get; }

    /// <summary>
    /// Gets the command middleware.
    /// </summary>
    public IList<CommandHandlerMiddlewareDelegate> Middleware { get; }

    /// <summary>
    /// Gets the command metadata.
    /// </summary>
    public IList<object> Metadata { get; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> associated with the application.
    /// </summary>
    public IServiceProvider ApplicationServices { get; }
}
