using System.Runtime.CompilerServices;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a service that can bind a command handler parameter to a value resolver.
/// </summary>
public interface ICommandHandlerParameterBinder
{
    /// <summary>
    /// Binds the specified parameter to a value resolver.
    /// </summary>
    /// <param name="context">The binder context.</param>
    /// <param name="defaultValueBox">The box containing the default value, if any.</param>
    /// <param name="description">The parameter description.</param>
    /// <returns>The parameter resolver.</returns>
    ICommandHandlerParameterResolver Bind(ICommandHandlerParameterBinderContext context, StrongBox<object?>? defaultValueBox, string? description);
}
