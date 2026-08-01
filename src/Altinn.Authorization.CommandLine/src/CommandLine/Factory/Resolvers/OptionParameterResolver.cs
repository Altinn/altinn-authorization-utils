using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Factory.Resolvers;

/// <summary>
/// Static factory for <see cref="OptionParameterResolver{T}"/>.
/// </summary>
internal static class OptionParameterResolver
{
    public static ICommandHandlerParameterResolver Create(ParameterInfo parameter, Option option, bool isRequired)
    {
        Debug.Assert(option is not null);
        Debug.Assert(option.ValueType == parameter.ParameterType);

        var resolverType = typeof(OptionParameterResolver<>).MakeGenericType(parameter.ParameterType);
        return (ICommandHandlerParameterResolver)Activator.CreateInstance(resolverType, parameter, option, isRequired)!;
    }
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a command line option for a specific parameter type.
/// </summary>
/// <typeparam name="T">The option type.</typeparam>
internal sealed class OptionParameterResolver<T>
    : ICommandHandlerParameterResolver
{
    private readonly ParameterInfo _parameter;
    private readonly Option<T> _option;
    private readonly bool _isRequired;

    public OptionParameterResolver(ParameterInfo parameter, Option<T> option, bool isRequired)
    {
        _parameter = parameter;
        _option = option;
        _isRequired = isRequired;
    }

    public Task<object?> ResolveParameterValue(CommandInvocationContext invocationContext, CancellationToken cancellationToken)
    {
        var result = invocationContext.ParseResult.GetResult(_option);
        if (result is null)
        {
            var parameterName = _parameter.Name!;
            var parameterTypeName = TypeNameHelper.GetTypeDisplayName(_parameter.ParameterType, fullName: false);
            ThrowHelper.ThrowInvalidOperationException($"Required option '{parameterTypeName} {parameterName}' was not provided.");
        }

        var value = result.GetValueOrDefault<T>();

        if (_isRequired && value is null)
        {
            var parameterName = _parameter.Name!;
            var parameterTypeName = TypeNameHelper.GetTypeDisplayName(_parameter.ParameterType, fullName: false);
            ThrowHelper.ThrowInvalidOperationException($"Required option '{parameterTypeName} {parameterName}' did not produce a non-null value.");
        }

        return Task.FromResult<object?>(value);
    }
}
