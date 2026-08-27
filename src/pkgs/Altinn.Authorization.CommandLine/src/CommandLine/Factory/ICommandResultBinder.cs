namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a service that can bind on a command handler based on the result provider.
/// </summary>
public interface ICommandResultBinder
{
    /// <summary>
    /// Binds the command handler to the result provider.
    /// </summary>
    /// <param name="context">The binder context.</param>
    void Bind(ICommandHandlerBinderContext context);
}
