using System.CommandLine;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a context for binding command handlers.
/// </summary>
public interface ICommandHandlerBinderContext
{
    /// <summary>
    /// Gets the application services.
    /// </summary>
    IServiceProvider ApplicationServices { get; }

    /// <summary>
    /// Adds the specified option to the command being built.
    /// </summary>
    /// <param name="option">The option to add.</param>
    void Add(Option option);

    /// <summary>
    /// Adds the specified argument to the command being built.
    /// </summary>
    /// <param name="argument">The argument to add.</param>
    void Add(Argument argument);
}
