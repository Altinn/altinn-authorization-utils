using System.Reflection;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a context for binding command handler parameters to value resolvers.
/// </summary>
public interface ICommandHandlerParameterBinderContext
    : ICommandHandlerBinderContext
{
    /// <summary>
    /// Gets the parameter being bound.
    /// </summary>
    ParameterInfo Parameter { get; }
}
