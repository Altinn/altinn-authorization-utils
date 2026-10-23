
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
    /// <param name="defaultValueProvider">The default-value provider, or null when no default is configured.</param>
    /// <param name="description">The parameter description.</param>
    /// <returns>The parameter resolver.</returns>
    ICommandHandlerParameterResolver Bind(ICommandHandlerParameterBinderContext context, DefaultValueProvider? defaultValueProvider, string? description);
}
