using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Factory.Resolvers;

/// <summary>
/// Static factory for <see cref="ArgumentParameterResolver{T}"/>.
/// </summary>
internal static class ArgumentParameterResolver
{
    public static ICommandHandlerParameterResolver Create(ParameterInfo parameter, Argument argument, bool isRequired)
    {
        Debug.Assert(argument is not null);
        Debug.Assert(argument.ValueType == parameter.ParameterType);

        var resolverType = typeof(ArgumentParameterResolver<>).MakeGenericType(parameter.ParameterType);
        return (ICommandHandlerParameterResolver)Activator.CreateInstance(resolverType, parameter, argument, isRequired)!;
    }
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a command line argument for a specific parameter type.
/// </summary>
/// <typeparam name="T">The argument type.</typeparam>
internal sealed class ArgumentParameterResolver<T>
    : ICommandHandlerParameterResolver
{
    private readonly ParameterInfo _parameter;
    private readonly Argument<T> _argument;
    private readonly bool _isRequired;

    public ArgumentParameterResolver(ParameterInfo parameter, Argument<T> argument, bool isRequired)
    {
        _parameter = parameter;
        _argument = argument;
        _isRequired = isRequired;
    }

    public Task<object?> ResolveParameterValue(CommandInvocationContext invocationContext, CancellationToken cancellationToken)
    {
        var result = invocationContext.ParseResult.GetResult(_argument);
        if (result is null)
        {
            var parameterName = _parameter.Name!;
            var parameterTypeName = TypeNameHelper.GetTypeDisplayName(_parameter.ParameterType, fullName: false);
            ThrowHelper.ThrowInvalidOperationException($"Required argument '{parameterTypeName} {parameterName}' was not provided.");
        }

        var value = result.GetValueOrDefault<T>();

        if (_isRequired && value is null)
        {
            var parameterName = _parameter.Name!;
            var parameterTypeName = TypeNameHelper.GetTypeDisplayName(_parameter.ParameterType, fullName: false);
            ThrowHelper.ThrowInvalidOperationException($"Required argument '{parameterTypeName} {parameterName}' did not produce a non-null value.");
        }

        return Task.FromResult<object?>(value);
    }
}
