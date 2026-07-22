using System.Collections.Concurrent;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.XmlDoc;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Provides a factory for creating <see cref="CommandHandlerDelegate"/> implementations for arbitrary invocation handlers.
/// </summary>
public static partial class CommandHandlerDelegateFactory
{
    private static readonly MethodInfo GetArgumentResultMethod = typeof(ParseResult).GetMethod(nameof(ParseResult.GetResult), [typeof(Argument)])!;
    private static readonly MethodInfo GetOptionResultMethod = typeof(ParseResult).GetMethod(nameof(ParseResult.GetResult), [typeof(Option)])!;
    private static readonly MethodInfo GetArgumentValueMethod = typeof(ArgumentResult).GetMethod(nameof(ArgumentResult.GetValueOrDefault))!;
    private static readonly MethodInfo GetOptionValueMethod = typeof(OptionResult).GetMethod(nameof(OptionResult.GetValueOrDefault))!;

    private static readonly MethodInfo ValueTaskAsTaskMethod = typeof(ValueTask).GetMethod(nameof(ValueTask.AsTask))!;

    private static readonly MethodInfo CheckSymbolResultIsNotNullMethod = GetMethodInfo<Action<SymbolResult?, string, string, string>>(
        static (symbolResult, parameterType, parameterName, sourceValue) => Check.SymbolResultIsNotNull(symbolResult, parameterType, parameterName, sourceValue));

    private static readonly ParameterExpression TargetExpr = Expression.Parameter(typeof(object), "target");
    private static readonly ParameterExpression InvocationContextExpr = Expression.Parameter(typeof(CommandInvocationContext), "invocationContext");
    private static readonly ParameterExpression CancellationTokenExpr = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

    private static readonly MemberExpression ParseResultExpr = Expression.Property(InvocationContextExpr, typeof(CommandInvocationContext).GetProperty(nameof(CommandInvocationContext.ParseResult))!);
    private static readonly MemberExpression ServiceProviderExpr = Expression.Property(InvocationContextExpr, typeof(CommandInvocationContext).GetProperty(nameof(CommandInvocationContext.ServiceProvider))!);
    private static readonly MemberExpression ConsoleExpr = Expression.Property(InvocationContextExpr, typeof(CommandInvocationContext).GetProperty(nameof(CommandInvocationContext.Console))!);
    private static readonly MemberExpression TaskCompletedTaskExpr = Expression.Property(null, typeof(Task).GetProperty(nameof(Task.CompletedTask))!);

    private static MethodInfo GetMethodInfo<T>(Expression<T> expr)
    {
        Debug.Assert(expr.Body is MethodCallExpression);
        var mc = (MethodCallExpression)expr.Body;
        Debug.Assert(mc.Object is null);
        return mc.Method;
    }

    /// <summary>
    /// Creates a <see cref="CommandHandlerDelegate"/> implementation for <paramref name="handler"/>.
    /// </summary>
    /// <param name="handler">An invocation handler with any number of custom parameters that often produces a response with its return value.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to configure the behavior of the handler.</param>
    /// <returns>The <see cref="CommandHandlerDelegate"/>.</returns>
    public static CommandHandlerDelegateResult Create(
        Delegate handler,
        IServiceProvider serviceProvider)
    {
        Guard.IsNotNull(handler);
        Guard.IsNotNull(serviceProvider);

        var targetExpression = handler.Target switch
        {
            object => Expression.Convert(TargetExpr, handler.Target.GetType()),
            null => null,
        };

        var factoryContext = CreateFactoryContext(serviceProvider);

        var targetableInvocationDelegate = handler is CommandHandlerDelegate
            ? null
            : CreateTargetableInvocationDelegate(handler.Method, targetExpression, factoryContext);

        CommandHandlerDelegate finalCommandHandlerDelegate = targetableInvocationDelegate switch
        {
            // handler is a CommandHandlerDelegate that has not been modified by a filter. Short-circuit and return the original CommandHandlerDelegate back.
            null => (CommandHandlerDelegate)handler,
            _ => (invocationContext, cancellationToken) => targetableInvocationDelegate.Value.Invoke(handler.Target, invocationContext, cancellationToken),
        };

        return new(
            finalCommandHandlerDelegate,
            [.. factoryContext.Options],
            [.. factoryContext.Arguments]);
    }

    /// <summary>
    /// Creates a <see cref="CommandHandlerDelegate"/> implementation for the specified <paramref name="handler"/> type and <paramref name="method"/>.
    /// </summary>
    /// <param name="handler">The command handler type.</param>
    /// <param name="method">The method info of the command handler.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to configure the behavior of the handler.</param>
    /// <returns>The <see cref="CommandHandlerDelegate"/>.</returns>
    public static CommandHandlerDelegateResult Create(
        Type handler,
        MethodInfo method,
        IServiceProvider serviceProvider)
    {
        Guard.IsNotNull(handler);
        Guard.IsNotNull(method);
        Guard.IsNotNull(serviceProvider);

        var factoryContext = CreateFactoryContext(serviceProvider);
        var targetExpression = Expression.Convert(TargetExpr, handler);
        var targetableInvocationDelegate = CreateTargetableInvocationDelegate(method, targetExpression, factoryContext);

        var objectFactory = ActivatorUtilities.CreateFactory(handler, []);
        CommandHandlerDelegate finalCommandHandlerDelegate = async (invocationContext, cancellationToken) =>
        {
            var target = objectFactory(invocationContext.ServiceProvider, []);
            try
            {
                await targetableInvocationDelegate.Value.Invoke(target, invocationContext, cancellationToken);
            }
            finally
            {
                if (target is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else if (target is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        };

        return new(
            finalCommandHandlerDelegate,
            [.. factoryContext.Options],
            [.. factoryContext.Arguments]);
    }

    private static Lazy<Func<object?, CommandInvocationContext, CancellationToken, Task>> CreateTargetableInvocationDelegate(
        MethodInfo methodInfo,
        Expression? targetExpression,
        CommandHandlerDelegateFactoryContext factoryContext)
    {
        factoryContext.ArgumentExpressions ??= CreateArguments(methodInfo.GetParameters(), factoryContext);
        var methodCall = CreateMethodCall(methodInfo, targetExpression, factoryContext.ArgumentExpressions);
        var returnType = methodInfo.ReturnType;

        var commandResultExpr = ConvertToTask(methodCall, ref returnType);
        var handleResultExpr = HandleCommandResult(commandResultExpr, returnType, factoryContext);

        var body = Expression.Block(
            factoryContext.ExtraLocals,
            [
                .. factoryContext.ResultAssignments,
                .. factoryContext.ResultCheckExpressions,
                .. factoryContext.ParamAssignments,
                .. factoryContext.ParamCheckExpressions,
                handleResultExpr
            ]);

        var lambda = Expression.Lambda<Func<object?, CommandInvocationContext, CancellationToken, Task>>(body, [TargetExpr, InvocationContextExpr, CancellationTokenExpr]);
        return new(lambda.Compile);
    }

    private static Expression HandleCommandResult(Expression commandResultExpr, Type resultType, CommandHandlerDelegateFactoryContext factoryContext)
    {
        if (resultType == typeof(void))
        {
            return commandResultExpr;
        }

        if (resultType.IsAssignableTo(typeof(ICommandResult)))
        {
            return Expression.Call(
                Generic.ForType(resultType).CommandResultExecute,
                commandResultExpr,
                InvocationContextExpr,
                CancellationTokenExpr);
        }

        var handlerType = typeof(ICommandResultHandler<>).MakeGenericType(resultType);
        if (factoryContext.ServiceProviderIsService is { } isServiceService && !isServiceService.IsService(handlerType))
        {
            ThrowHelper.ThrowInvalidOperationException($"The return type '{TypeNameHelper.GetTypeDisplayName(resultType, fullName: false)}' is not supported. The return type must be 'void', implement '{TypeNameHelper.GetTypeDisplayName(typeof(ICommandResult), fullName: false)}', or have a registered handler implementing '{TypeNameHelper.GetTypeDisplayName(handlerType, fullName: false)}'.");
        }

        // if we don't have access to a service-check, we assume the handler is registered and will throw an exception if it is not.
        return Expression.Call(
            Generic.ForType(resultType).HandleResultMethod,
            commandResultExpr,
            InvocationContextExpr,
            CancellationTokenExpr);
    }

    private static Expression ConvertToTask(Expression methodCall, ref Type resultType)
    {
        // void cases
        if (resultType == typeof(Task))
        {
            resultType = typeof(void);
            return methodCall;
        }

        if (resultType == typeof(ValueTask))
        {
            resultType = typeof(void);
            return Expression.Call(
                methodCall,
                ValueTaskAsTaskMethod);
        }

        if (resultType == typeof(void))
        {
            return Expression.Block([
                methodCall,
                TaskCompletedTaskExpr,
            ]);
        }

        // with task-like return value
        if (resultType.IsConstructedGenericType)
        {
            var genericTypeDefinition = resultType.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(Task<>))
            {
                resultType = resultType.GetGenericArguments()[0];
                return methodCall;
            }

            if (genericTypeDefinition == typeof(ValueTask<>))
            {
                resultType = resultType.GetGenericArguments()[0];
                return Expression.Call(
                    methodCall,
                    Generic.ForType(resultType).ValueTaskAsTaskMethod);
            }
        }

        // naked return value
        return Expression.Call(
            Generic.ForType(resultType).TaskFromResultMethod,
            methodCall);
    }

    private static Expression CreateMethodCall(MethodInfo methodInfo, Expression? target, Expression[] arguments)
        => target is null
        ? Expression.Call(methodInfo, arguments)
        : Expression.Call(target, methodInfo, arguments);

    private static Expression[] CreateArguments(ParameterInfo[]? parameters, CommandHandlerDelegateFactoryContext factoryContext)
    {
        if (parameters is null || parameters.Length == 0)
        {
            return [];
        }

        var args = new Expression[parameters.Length];
        factoryContext.ArgumentTypes = new Type[parameters.Length];
        factoryContext.BoxedArgs = new Expression[parameters.Length];
        factoryContext.Parameters = new List<ParameterInfo>(parameters);

        for (var i = 0; i < parameters.Length; i++)
        {
            args[i] = CreateArgument(parameters[i], factoryContext);
            factoryContext.ArgumentTypes[i] = parameters[i].ParameterType;
            factoryContext.BoxedArgs[i] = Expression.Convert(args[i], typeof(object));
        }

        return args;
    }

    private static Expression CreateArgument(
        ParameterInfo parameter,
        CommandHandlerDelegateFactoryContext factoryContext)
    {
        if (parameter.Name is null)
        {
            ThrowHelper.ThrowInvalidOperationException($"Encountered a parameter of type '{parameter.ParameterType}' without a name. Parameters must have a name.");
        }

        if (parameter.ParameterType.IsByRef)
        {
            var attribute = "ref";

            if (parameter.Attributes.HasFlag(ParameterAttributes.In))
            {
                attribute = "in";
            }
            else if (parameter.Attributes.HasFlag(ParameterAttributes.Out))
            {
                attribute = "out";
            }

            ThrowHelper.ThrowNotSupportedException($"The by reference parameter '{attribute} {TypeNameHelper.GetTypeDisplayName(parameter.ParameterType, fullName: false)} {parameter.Name}' is not supported.");
        }

        var parameterCustomAttributes = parameter.GetCustomAttributes();
        if (parameterCustomAttributes.OfType<IFromArgumentMetadata>().FirstOrDefault() is { } argumentMetadata)
        {
            var isOptional = IsOptionalParameter(parameter, factoryContext);
            var argument = CreateCliArgument(factoryContext, parameter, argumentMetadata, isOptional);

            Debug.Assert(argument is not null);
            Debug.Assert(argument.ValueType == parameter.ParameterType);

            var argumentResultExpr = Expression.Call(ParseResultExpr, GetArgumentResultMethod, Expression.Constant(argument));
            return BindParameterFromArgumentResult(parameter, argumentResultExpr, isOptional, factoryContext, "argument");
        }

        if (parameterCustomAttributes.OfType<IFromOptionMetadata>().FirstOrDefault() is { } optionMetadata)
        {
            var isOptional = IsOptionalParameter(parameter, factoryContext);
            var option = CreateCliOption(factoryContext, parameter, optionMetadata, isOptional);

            Debug.Assert(option is not null);
            Debug.Assert(option.ValueType == parameter.ParameterType);

            var optionResultExpr = Expression.Call(ParseResultExpr, GetOptionResultMethod, Expression.Constant(option));
            return BindParameterFromOptionResult(parameter, optionResultExpr, isOptional, factoryContext, "option");
        }

        if (parameterCustomAttributes.OfType<IFromServiceMetadata>().FirstOrDefault() is { } serviceMetadata)
        {
            return BindParameterFromService(parameter, serviceMetadata.ServiceKey, factoryContext);
        }

        if (parameter.ParameterType == typeof(CommandInvocationContext))
        {
            return InvocationContextExpr;
        }

        if (parameter.ParameterType == typeof(CancellationToken))
        {
            return CancellationTokenExpr;
        }

        if (parameter.ParameterType == typeof(IConsole))
        {
            return ConsoleExpr;
        }

        if (parameter.ParameterType == typeof(ParseResult))
        {
            return ParseResultExpr;
        }

        if (parameter.ParameterType == typeof(IServiceProvider))
        {
            return ServiceProviderExpr;
        }

        if (factoryContext.ServiceProviderIsService is { } isServiceService
            && isServiceService.IsService(parameter.ParameterType))
        {
            return BindParameterFromService(parameter, null, factoryContext);
        }

        return ThrowHelper.ThrowNotSupportedException<Expression>($"The parameter '{TypeNameHelper.GetTypeDisplayName(parameter.ParameterType, fullName: false)} {parameter.Name}' is not supported. Parameters must be bound from an argument, option, service, or be of a known type.");
    }

    private static Expression BindParameterFromService(
        ParameterInfo parameter,
        object? serviceKey,
        CommandHandlerDelegateFactoryContext factoryContext)
    {
        var isOptional = IsOptionalParameter(parameter, factoryContext);

        if (isOptional)
        {
            return serviceKey is null
                ? Expression.Call(
                    Generic.ForType(parameter.ParameterType).GetServiceMethod,
                    ServiceProviderExpr)
                : Expression.Call(
                    Generic.ForType(parameter.ParameterType).GetKeyedServiceMethod,
                    ServiceProviderExpr,
                    Expression.Constant(serviceKey));
        }

        return serviceKey is null
            ? Expression.Call(
                Generic.ForType(parameter.ParameterType).GetRequiredServiceMethod,
                ServiceProviderExpr)
            : Expression.Call(
                Generic.ForType(parameter.ParameterType).GetRequiredKeyedServiceMethod,
                ServiceProviderExpr,
                Expression.Constant(serviceKey));
    }

    private static Expression BindParameterFromArgumentResult(
        ParameterInfo parameter,
        Expression argumentResultExpr,
        bool isOptional,
        CommandHandlerDelegateFactoryContext factoryContext,
        string source)
    {
        var result = Expression.Variable(typeof(ArgumentResult), $"{parameter.Name}Result");
        factoryContext.ExtraLocals.Add(result);
        factoryContext.ResultAssignments.Add(Expression.Assign(result, argumentResultExpr));

        var parameterTypeNameConstant = Expression.Constant(TypeNameHelper.GetTypeDisplayName(parameter.ParameterType, fullName: false));
        var parameterNameConstant = Expression.Constant(parameter.Name);
        var sourceConstant = Expression.Constant(source);
        factoryContext.ResultCheckExpressions.Add(
            Expression.Call(
                CheckSymbolResultIsNotNullMethod,
                result,
                parameterTypeNameConstant,
                parameterNameConstant,
                sourceConstant));

        var getArgumentValueMethod = GetArgumentValueMethod.MakeGenericMethod(parameter.ParameterType);
        var valueExpr = Expression.Call(result, getArgumentValueMethod);

        return BindParameterFromValueExpression(
            parameter,
            valueExpr,
            isOptional,
            factoryContext,
            parameterTypeNameConstant,
            parameterNameConstant,
            sourceConstant);
    }

    private static Expression BindParameterFromOptionResult(
        ParameterInfo parameter,
        Expression optionResultExpr,
        bool isOptional,
        CommandHandlerDelegateFactoryContext factoryContext,
        string source)
    {
        var result = Expression.Variable(typeof(OptionResult), $"{parameter.Name}Result");
        factoryContext.ExtraLocals.Add(result);
        factoryContext.ResultAssignments.Add(Expression.Assign(result, optionResultExpr));

        var parameterTypeNameConstant = Expression.Constant(TypeNameHelper.GetTypeDisplayName(parameter.ParameterType, fullName: false));
        var parameterNameConstant = Expression.Constant(parameter.Name);
        var sourceConstant = Expression.Constant(source);
        factoryContext.ResultCheckExpressions.Add(
            Expression.Call(
                CheckSymbolResultIsNotNullMethod,
                result,
                parameterTypeNameConstant,
                parameterNameConstant,
                sourceConstant));

        var getOptionValueMethod = GetOptionValueMethod.MakeGenericMethod(parameter.ParameterType);
        var valueExpr = Expression.Call(result, getOptionValueMethod);

        return BindParameterFromValueExpression(
            parameter,
            valueExpr,
            isOptional,
            factoryContext,
            parameterTypeNameConstant,
            parameterNameConstant,
            sourceConstant);
    }

    private static Expression BindParameterFromValueExpression(
        ParameterInfo parameter,
        Expression valueExpression,
        bool isOptional,
        CommandHandlerDelegateFactoryContext factoryContext,
        ConstantExpression parameterTypeNameConstant,
        ConstantExpression parameterNameConstant,
        ConstantExpression sourceConstant)
    {
        var argument = Expression.Variable(parameter.ParameterType, $"{parameter.Name}Value");
        factoryContext.ExtraLocals.Add(argument);
        factoryContext.ParamAssignments.Add(Expression.Assign(argument, valueExpression));

        if (!isOptional && !parameter.ParameterType.IsValueType)
        {
            factoryContext.ParamCheckExpressions.Add(
                Expression.Call(
                Generic.ForType(parameter.ParameterType).CheckParameterIsNotNullMethod,
                argument,
                parameterTypeNameConstant,
                parameterNameConstant,
                sourceConstant));
        }

        return argument;
    }

    private static bool IsOptionalParameter(ParameterInfo parameter, CommandHandlerDelegateFactoryContext factoryContext)
    {
        // - Parameters representing value or reference types with a default value
        // under any nullability context are treated as optional.
        // - Value type parameters without a default value in an oblivious
        // nullability context are required.
        // - Reference type parameters without a default value in an oblivious
        // nullability context are optional.
        var nullabilityInfo = factoryContext.NullabilityContext.Create(parameter);
        return parameter.HasDefaultValue
            || nullabilityInfo.ReadState != NullabilityState.NotNull;
    }

    private static Argument CreateCliArgument(
        CommandHandlerDelegateFactoryContext factoryContext,
        ParameterInfo parameter,
        IFromArgumentMetadata argumentMetadata,
        bool isOptional)
    {
        if (string.IsNullOrEmpty(parameter.Name))
        {
            ThrowHelper.ThrowInvalidOperationException($"Encountered a parameter of type '{TypeNameHelper.GetTypeDisplayName(parameter.ParameterType, fullName: false)}' without a name. Parameters must have a name.");
        }

        StrongBox<object?>? defaultValueBox = null;
        if (isOptional)
        {
            defaultValueBox = new();

            if (parameter.HasDefaultValue)
            {
                defaultValueBox.Value = parameter.DefaultValue;
            }
        }

        var argument = argumentMetadata.CreateArgument(
            serviceProvider: factoryContext.ServiceProvider,
            parameterType: parameter.ParameterType,
            parameterName: parameter.Name,
            defaultValueBox: defaultValueBox);

        if (factoryContext.Arguments.Any(a => a.Name == argument.Name))
        {
            ThrowHelper.ThrowInvalidOperationException($"An argument with the name '{argument.Name}' has already been added to the command.");
        }

        if (argument.Description is null && factoryContext.XmlDocProvider.TryGetXmlDoc(parameter, out var xmlDoc))
        {
            argument.Description = XmlCommentsTextHelper.Humanize(xmlDoc.InnerXml);
        }

        factoryContext.Arguments.Add(argument);
        return argument;
    }

    private static Option CreateCliOption(
        CommandHandlerDelegateFactoryContext factoryContext,
        ParameterInfo parameter,
        IFromOptionMetadata optionMetadata,
        bool isOptional)
    {
        if (string.IsNullOrEmpty(parameter.Name))
        {
            ThrowHelper.ThrowInvalidOperationException($"Encountered a parameter of type '{TypeNameHelper.GetTypeDisplayName(parameter.ParameterType, fullName: false)}' without a name. Parameters must have a name.");
        }

        StrongBox<object?>? defaultValueBox = null;
        if (isOptional)
        {
            defaultValueBox = new();

            if (parameter.HasDefaultValue)
            {
                defaultValueBox.Value = parameter.DefaultValue;
            }
        }

        var option = optionMetadata.CreateOption(
            serviceProvider: factoryContext.ServiceProvider,
            parameterType: parameter.ParameterType,
            parameterName: parameter.Name,
            defaultValueBox: defaultValueBox);

        if (factoryContext.Options.Any(o => o.Name == option.Name))
        {
            ThrowHelper.ThrowInvalidOperationException($"An option with the name '{option.Name}' has already been added to the command.");
        }

        if (option.Description is null && factoryContext.XmlDocProvider.TryGetXmlDoc(parameter, out var xmlDoc))
        {
            option.Description = XmlCommentsTextHelper.Humanize(xmlDoc.InnerXml);
        }

        factoryContext.Options.Add(option);
        return option;
    }

    private static CommandHandlerDelegateFactoryContext CreateFactoryContext(
        IServiceProvider serviceProvider)
    {
        return new CommandHandlerDelegateFactoryContext
        {
            ServiceProvider = serviceProvider,
            ServiceProviderIsService = serviceProvider.GetService<IServiceProviderIsService>(),
            XmlDocProvider = serviceProvider.GetRequiredService<IXmlDocProvider>(),
        };
    }

    private abstract class Generic
    {
        private static readonly ConcurrentDictionary<Type, Generic> _factories = new();

        public static Generic ForType(Type type)
            => _factories.GetOrAdd(type, t => (Generic)Activator.CreateInstance(typeof(Generic<>).MakeGenericType(t))!);

        public abstract MethodInfo CheckParameterIsNotNullMethod { get; }

        public abstract MethodInfo ValueTaskAsTaskMethod { get; }

        public abstract MethodInfo TaskFromResultMethod { get; }

        public abstract MethodInfo HandleResultMethod { get; }

        public abstract MethodInfo GetServiceMethod { get; }

        public abstract MethodInfo GetKeyedServiceMethod { get; }

        public abstract MethodInfo GetRequiredServiceMethod { get; }

        public abstract MethodInfo GetRequiredKeyedServiceMethod { get; }

        public abstract MethodInfo CommandResultExecute { get; }
    }

    private sealed class Generic<T>
        : Generic
        where T : notnull
    {
        public override MethodInfo CheckParameterIsNotNullMethod { get; } = GetMethodInfo<Action<T?, string, string, string>>(
            static (parameterValue, parameterTypeName, parameterName, source) => Check.ParameterIsNotNull(parameterValue, parameterTypeName, parameterName, source));

        public override MethodInfo ValueTaskAsTaskMethod { get; } = typeof(ValueTask<T>).GetMethod(nameof(ValueTask<>.AsTask))!;

        public override MethodInfo TaskFromResultMethod { get; } = GetMethodInfo<Func<T, Task<T>>>(
            static (result) => Task.FromResult(result));

        public override MethodInfo HandleResultMethod { get; } = GetMethodInfo<Func<Task<T>, CommandInvocationContext, CancellationToken, Task>>(
            static (commandResult, invocationContext, cancellationToken) => Handle.HandleHandlerResult(commandResult, invocationContext, cancellationToken));

        public override MethodInfo GetServiceMethod { get; } = GetMethodInfo<Func<IServiceProvider, T?>>(
            static (sp) => sp.GetService<T>());

        public override MethodInfo GetKeyedServiceMethod { get; } = GetMethodInfo<Func<IServiceProvider, object, T?>>(
            static (sp, key) => sp.GetKeyedService<T>(key));

        public override MethodInfo GetRequiredServiceMethod { get; } = GetMethodInfo<Func<IServiceProvider, T>>(
            static (sp) => sp.GetRequiredService<T>());

        public override MethodInfo GetRequiredKeyedServiceMethod { get; } = GetMethodInfo<Func<IServiceProvider, object, T>>(
            static (sp, key) => sp.GetRequiredKeyedService<T>(key));

        public override MethodInfo CommandResultExecute { get; } = GetMethodInfo<Func<Task<T>, CommandInvocationContext, CancellationToken, Task>>(
            static (commandResult, invocationContext, cancellationToken) => Handle.HandleCommandResult(commandResult, invocationContext, cancellationToken));
    }

    private static partial class Check
    {
        public static void SymbolResultIsNotNull(SymbolResult? symbolResult, string parameterTypeName, string parameterName, string source)
        {
            if (symbolResult is null)
            {
                ThrowHelper.ThrowInvalidOperationException($"Required symbol '{parameterTypeName} {parameterName}' was not provided from {source}.");
            }
        }

        public static void ParameterIsNotNull<T>(T? parameterValue, string parameterTypeName, string parameterName, string source)
        {
            if (parameterValue is null)
            {
                ThrowHelper.ThrowInvalidOperationException($"Required parameter '{parameterTypeName} {parameterName}' was not provided from {source}.");
            }
        }
    }

    private static partial class Handle
    {
        public static async Task HandleCommandResult<T>(Task<T> commandResult, CommandInvocationContext invocationContext, CancellationToken cancellationToken)
        {
            var result = await commandResult;
            await ((ICommandResult)result!).Execute(invocationContext, cancellationToken);
        }

        public static async Task HandleHandlerResult<T>(Task<T> commandResult, CommandInvocationContext invocationContext, CancellationToken cancellationToken)
        {
            var handler = invocationContext.ServiceProvider.GetRequiredService<ICommandResultHandler<T>>();
            var result = await commandResult;
            await handler.HandleResult(result, invocationContext, cancellationToken);
        }
    }
}
