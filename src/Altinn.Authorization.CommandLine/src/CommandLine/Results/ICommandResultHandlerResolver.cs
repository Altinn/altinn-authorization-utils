using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Resolves command result handlers for specific result types.
/// </summary>
public interface ICommandResultHandlerResolver
{
    /// <summary>
    /// Attempts to resolve a command result handler for the specified result type.
    /// </summary>
    /// <param name="type">The result type.</param>
    /// <param name="handler">The resolved handler, when available.</param>
    /// <returns><see langword="true"/> if a handler was resolved; otherwise, <see langword="false"/>.</returns>
    bool TryResolve(Type type, [NotNullWhen(true)] out ICommandResultHandler? handler);
}
