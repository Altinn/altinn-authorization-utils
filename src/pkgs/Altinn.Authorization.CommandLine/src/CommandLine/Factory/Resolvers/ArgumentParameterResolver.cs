using System.CommandLine;
using System.Diagnostics;
using System.Reflection;

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
        return (ICommandHandlerParameterResolver)Activator.CreateInstance(resolverType, argument, isRequired)!;
    }
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a command line argument for a specific parameter type.
/// </summary>
/// <typeparam name="T">The argument type.</typeparam>
internal sealed class ArgumentParameterResolver<T>
    : ICommandHandlerParameterResolver
{
    private readonly Argument<T> _argument;
    private readonly bool _isRequired;

    public ArgumentParameterResolver(Argument<T> argument, bool isRequired)
    {
        _argument = argument;
        _isRequired = isRequired;
    }

    public Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
    {
        var result = context.ParseResult.GetResult(_argument);
        if (result is null)
        {
            context.AddError($"Required argument '{_argument.Name}' was not provided.");
            return Task.CompletedTask;
        }

        var value = result.GetValueOrDefault<T>();

        if (_isRequired && value is null)
        {
            context.AddError($"Required argument '{_argument.Name}' did not produce a non-null value.");
            return Task.CompletedTask;
        }

        context.SetParameterValue(value);
        return Task.CompletedTask;
    }
}
