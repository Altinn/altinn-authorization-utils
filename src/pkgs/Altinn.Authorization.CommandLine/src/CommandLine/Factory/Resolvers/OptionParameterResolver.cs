using System.CommandLine;
using System.Diagnostics;
using System.Reflection;

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
        return (ICommandHandlerParameterResolver)Activator.CreateInstance(resolverType, option, isRequired)!;
    }
}

/// <summary>
/// Defines a parameter resolver that resolves the value of a command line option for a specific parameter type.
/// </summary>
/// <typeparam name="T">The option type.</typeparam>
internal sealed class OptionParameterResolver<T>
    : ICommandHandlerParameterResolver
{
    private readonly Option<T> _option;
    private readonly bool _isRequired;

    public OptionParameterResolver(Option<T> option, bool isRequired)
    {
        _option = option;
        _isRequired = isRequired;
    }

    public Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
    {
        var result = context.ParseResult.GetResult(_option);
        if (result is null)
        {
            context.AddError($"Required option '{_option.Name}' was not provided.");
            return Task.CompletedTask;
        }

        var value = result.GetValueOrDefault<T>();

        if (_isRequired && value is null)
        {
            context.AddError($"Required option '{_option.Name}' did not produce a non-null value.");
            return Task.CompletedTask;
        }

        context.SetParameterValue(value);
        return Task.CompletedTask;
    }
}
