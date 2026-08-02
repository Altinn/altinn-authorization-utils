using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Altinn.Authorization.CommandLine.Factory;

internal sealed class CommandHandlerParameterBinderResolver
{
    private readonly ImmutableArray<ICommandHandlerParameterBinderResolver> _parameterResolvers;

    public CommandHandlerParameterBinderResolver(IEnumerable<ICommandHandlerParameterBinderResolver> parameterResolvers)
    {
        _parameterResolvers = parameterResolvers.ToImmutableArray();
    }

    public bool TryResolve(ParameterInfo parameter, [NotNullWhen(true)] out ICommandHandlerParameterBinder? parameterBinder)
    {
        foreach (var resolver in _parameterResolvers)
        {
            if (resolver.TryResolve(parameter, out parameterBinder))
            {
                return true;
            }
        }

        parameterBinder = null;
        return false;
    }
}
