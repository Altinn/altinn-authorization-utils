namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Builds conventions that will be used for customization of <see cref="ICommandBuilder"/> instances.
/// </summary>
/// <remarks>
/// This interface is used at application startup to customize commands for the application.
/// </remarks>
public interface ICommandConventionBuilder
{
    /// <summary>
    /// Adds the specified convention to the builder. Conventions are used to customize <see cref="ICommandBuilder"/> instances.
    /// </summary>
    /// <param name="convention">The convention to add to the builder.</param>
    /// <param name="recursive">If true, the convention will be applied to all child commands. If false, the convention will only be applied to the current command.</param>
    void Add(Action<ICommandBuilder> convention, bool recursive = false);

    /// <summary>
    /// Registers the specified convention for execution after conventions registered
    /// via <see cref="Add(Action{ICommandBuilder}, bool)"/>
    /// </summary>
    /// <param name="finallyConvention">The convention to add to the builder.</param>
    /// <param name="recursive">If true, the convention will be applied to all child commands. If false, the convention will only be applied to the current command.</param>
    void Finally(Action<ICommandBuilder> finallyConvention, bool recursive = false);
}
