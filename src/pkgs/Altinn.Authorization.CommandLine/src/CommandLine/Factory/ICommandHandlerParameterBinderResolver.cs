using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// A resolver for <see cref="ICommandHandlerParameterBinder"/>s.
/// </summary>
public interface ICommandHandlerParameterBinderResolver
{
    /// <summary>
    /// Attempts to resolve a <see cref="ICommandHandlerParameterBinder"/> for the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to bind.</param>
    /// <param name="parameterBinder">The resolved parameter binder, if any.</param>
    /// <returns><see langword="true"/> if a parameter binder was resolved; otherwise, <see langword="false"/>.</returns>
    bool TryResolve(ParameterInfo parameter, [NotNullWhen(true)] out ICommandHandlerParameterBinder? parameterBinder);
}
