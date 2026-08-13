using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Resolves command result handlers from registered resolvers.
/// </summary>
public sealed class CommandResultHandlerResolver
{
    private readonly ImmutableArray<ICommandResultHandlerResolver> _resolvers;
    private readonly Shim _shim;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResultHandlerResolver"/> class.
    /// </summary>
    /// <param name="resolvers">The registered command result handler resolvers.</param>
    public CommandResultHandlerResolver(IEnumerable<ICommandResultHandlerResolver> resolvers)
    {
        _resolvers = [.. resolvers];
        _shim = new Shim(this);
    }

    /// <summary>
    /// Attempts to resolve a command result handler for the specified result type.
    /// </summary>
    /// <param name="type">The result type.</param>
    /// <param name="handler">The resolved handler, when available.</param>
    /// <returns><see langword="true"/> if a handler was resolved; otherwise, <see langword="false"/>.</returns>
    public bool TryResolve(Type type, [NotNullWhen(true)] out ICommandResultHandler? handler)
    {
        foreach (var resolver in _resolvers)
        {
            if (resolver.TryResolve(_shim, type, out handler))
            {
                return true;
            }
        }

        handler = null!;
        return false;
    }

    private sealed class Shim
        : ICommandResultHandlerResolver
    {
        private readonly CommandResultHandlerResolver _resolver;

        public Shim(CommandResultHandlerResolver resolver)
        {
            _resolver = resolver;
        }

        public bool TryResolve(ICommandResultHandlerResolver resolvers, Type type, [NotNullWhen(true)] out ICommandResultHandler? handler)
            => _resolver.TryResolve(type, out handler);
    }
}
