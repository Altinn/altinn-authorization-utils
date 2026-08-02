using System.Collections.Concurrent;
using System.CommandLine;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Factory.Resolvers;
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
    private static readonly MethodInfo ValueTaskAsTaskMethod = typeof(ValueTask).GetMethod(nameof(ValueTask.AsTask))!;
    private static readonly MethodInfo ParameterResolverResolveMethod = typeof(ICommandHandlerParameterResolver).GetMethod(nameof(ICommandHandlerParameterResolver.ResolveParameterValue))!;
    private static readonly MethodInfo HandleInvokeMethod = typeof(Handle).GetMethod(nameof(Handle.Invoke))!;

    private static readonly ParameterExpression TargetExpr = Expression.Parameter(typeof(object), "target");
    private static readonly ParameterExpression InvocationContextExpr = Expression.Parameter(typeof(CommandInvocationContext), "invocationContext");
    private static readonly ParameterExpression CancellationTokenExpr = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

    private static readonly MemberExpression TaskCompletedTaskExpr = Expression.Property(null, typeof(Task).GetProperty(nameof(Task.CompletedTask))!);

    private static readonly Expression EmptyObjectTaskArrayExpr = Expression.Call(
        typeof(Array).GetMethod(nameof(Array.Empty), BindingFlags.Static | BindingFlags.Public)!.MakeGenericMethod(typeof(Task<object>)));

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
            factoryContext.Options,
            factoryContext.Arguments,
            factoryContext.Metadata);
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
            var target = objectFactory(invocationContext.ApplicationServices, []);
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
            factoryContext.Options,
            factoryContext.Arguments,
            factoryContext.Metadata);
    }

    private static Lazy<Func<object?, CommandInvocationContext, CancellationToken, Task>> CreateTargetableInvocationDelegate(
        MethodInfo methodInfo,
        Expression? targetExpression,
        CommandHandlerDelegateFactoryContext factoryContext)
    {
        factoryContext.ArgumentExpressions ??= CreateArguments(methodInfo.GetParameters(), factoryContext);
        var innerHandler = CreateInnerHandler(methodInfo, targetExpression, factoryContext);

        var arrayExpression = factoryContext.ArgumentExpressions.Length > 0
            ? Expression.NewArrayInit(typeof(Task<object?>), factoryContext.ArgumentExpressions)
            : EmptyObjectTaskArrayExpr;

        var body = Expression.Call(
            HandleInvokeMethod,
            TargetExpr,
            arrayExpression,
            innerHandler);

        var lambda = Expression.Lambda<Func<object?, CommandInvocationContext, CancellationToken, Task>>(body, [TargetExpr, InvocationContextExpr, CancellationTokenExpr]);
        return new(lambda.Compile);
    }

    private static Expression CreateInnerHandler(
        MethodInfo methodInfo,
        Expression? targetExpression,
        CommandHandlerDelegateFactoryContext factoryContext)
    {
        var arrayParameter = Expression.Parameter(typeof(object[]), "args");
        var args = new Expression[factoryContext.ArgumentExpressions!.Length];
        for (var i = 0; i < factoryContext.ArgumentExpressions.Length; i++)
        {
            args[i] = Expression.Convert(
                Expression.ArrayIndex(arrayParameter, Expression.Constant(i)),
                factoryContext.ArgumentTypes[i]);
        }

        var methodCall = CreateMethodCall(methodInfo, targetExpression, args);
        var returnType = methodInfo.ReturnType;

        var commandResultExpr = ConvertToTask(methodCall, ref returnType);
        var handleResultExpr = HandleCommandResult(commandResultExpr, returnType, factoryContext);

        return Expression.Lambda<Func<object?, object?[], Task>>(handleResultExpr, TargetExpr, arrayParameter);
    }

    private static Expression HandleCommandResult(Expression commandResultExpr, Type resultType, CommandHandlerDelegateFactoryContext factoryContext)
    {
        factoryContext.Metadata.Add(new ResultTypeMetadata(resultType));
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

        if (!factoryContext.ResultHandler.TryResolve(resultType, out var handler))
        {
            ThrowHelper.ThrowInvalidOperationException($"The return type '{TypeNameHelper.GetTypeDisplayName(resultType, fullName: false)}' is not supported. The return type must be 'void', implement '{TypeNameHelper.GetTypeDisplayName(typeof(ICommandResult), fullName: false)}', or have a registered handler resolver implementing '{TypeNameHelper.GetTypeDisplayName(typeof(ICommandResultHandlerResolver), fullName: false)}'.");
        }

        factoryContext.Metadata.Add(new ResultHandlerMetadata(handler));
        if (handler is ICommandResultBinder resultBinder)
        {
            resultBinder.Bind(new BinderBuilder(factoryContext));
        }

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
        factoryContext.Parameters = new List<ParameterInfo>(parameters);

        for (var i = 0; i < parameters.Length; i++)
        {
            args[i] = CreateArgument(parameters[i], factoryContext);
            factoryContext.ArgumentTypes[i] = parameters[i].ParameterType;
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

        var resolver = GetParameterResolver(parameter, factoryContext);
        var resolverExpression = Expression.Constant(resolver, typeof(ICommandHandlerParameterResolver));
        return Expression.Call(
            resolverExpression,
            ParameterResolverResolveMethod,
            InvocationContextExpr,
            CancellationTokenExpr);
    }

    private static ICommandHandlerParameterResolver GetParameterResolver(
        ParameterInfo parameter,
        CommandHandlerDelegateFactoryContext factoryContext)
    {
        var parameterCustomAttributes = parameter.GetCustomAttributes();
        var isOptional = IsOptionalParameter(parameter, factoryContext);

        if (parameterCustomAttributes.OfType<ICommandHandlerParameterBinder>().FirstOrDefault() is { } parameterHandler)
        {
            return BindParameter(parameterHandler, parameter, factoryContext, isOptional);
        }

        if (parameterCustomAttributes.OfType<IFromArgumentMetadata>().FirstOrDefault() is { } argumentMetadata)
        {
            var argument = CreateCliArgument(factoryContext, parameter, argumentMetadata, isOptional);

            Debug.Assert(argument is not null);
            Debug.Assert(argument.ValueType == parameter.ParameterType);

            return ArgumentParameterResolver.Create(parameter, argument, isRequired: !isOptional);
        }

        if (parameterCustomAttributes.OfType<IFromOptionMetadata>().FirstOrDefault() is { } optionMetadata)
        {
            var option = CreateCliOption(factoryContext, parameter, optionMetadata, isOptional);

            Debug.Assert(option is not null);
            Debug.Assert(option.ValueType == parameter.ParameterType);

            return OptionParameterResolver.Create(parameter, option, isRequired: !isOptional);
        }

        if (parameterCustomAttributes.OfType<IFromServiceMetadata>().FirstOrDefault() is { } serviceMetadata)
        {
            return new ServiceParameterResolver(parameter.ParameterType, serviceMetadata.ServiceKey, !isOptional);
        }

        if (factoryContext.ParameterBinderResolver.TryResolve(parameter, out var parameterBinder))
        {
            return BindParameter(parameterBinder, parameter, factoryContext, isOptional);
        }

        if (parameter.ParameterType == typeof(CommandInvocationContext))
        {
            return CommandInvocationContextParameterResolver.Instance;
        }

        if (parameter.ParameterType == typeof(CancellationToken))
        {
            return CancellationTokenParameterResolver.Instance;
        }

        if (parameter.ParameterType == typeof(IConsole))
        {
            return ConsoleParameterResolver.Instance;
        }

        if (parameter.ParameterType == typeof(ParseResult))
        {
            return ParseResultParameterResolver.Instance;
        }

        if (parameter.ParameterType == typeof(IServiceProvider))
        {
            return ApplicationServicesParameterResolver.Instance;
        }

        if (factoryContext.ServiceProviderIsService is { } isServiceService
            && isServiceService.IsService(parameter.ParameterType))
        {
            return new ServiceParameterResolver(parameter.ParameterType, serviceKey: null, !isOptional);
        }

        return ThrowHelper.ThrowNotSupportedException<ICommandHandlerParameterResolver>($"The parameter '{TypeNameHelper.GetTypeDisplayName(parameter.ParameterType, fullName: false)} {parameter.Name}' is not supported. Parameters must be bound from an argument, option, service, or be of a known type.");
    }

    private static ICommandHandlerParameterResolver BindParameter(
        ICommandHandlerParameterBinder parameterBinder,
        ParameterInfo parameter,
        CommandHandlerDelegateFactoryContext factoryContext,
        bool isOptional)
    {
        StrongBox<object?>? defaultValueBox = null;
        if (isOptional)
        {
            defaultValueBox = new();

            if (parameter.HasDefaultValue)
            {
                defaultValueBox.Value = parameter.DefaultValue;
            }
        }

        string? argumentDescription = null;

        if (factoryContext.XmlDocProvider.TryGetXmlDoc(parameter, out var xmlDoc))
        {
            argumentDescription = XmlCommentsTextHelper.Humanize(xmlDoc.InnerXml);
        }

        var builder = new ParameterBinderBuilder(parameter, factoryContext);
        return parameterBinder.Bind(builder, defaultValueBox, argumentDescription);
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
            ResultHandler = serviceProvider.GetRequiredService<CommandResultHandler>(),
            ParameterBinderResolver = serviceProvider.GetRequiredService<CommandHandlerParameterBinderResolver>(),
            XmlDocProvider = serviceProvider.GetRequiredService<IXmlDocProvider>(),
        };
    }

    private class BinderBuilder(CommandHandlerDelegateFactoryContext factoryContext)
        : ICommandHandlerBinderContext
    {
        public IServiceProvider ApplicationServices
            => factoryContext.ServiceProvider;

        public void Add(Option option)
        {
            if (factoryContext.Options.Any(o => o.Name == option.Name))
            {
                ThrowHelper.ThrowInvalidOperationException($"An option with the name '{option.Name}' has already been added to the command.");
            }

            factoryContext.Options.Add(option);
        }

        public void Add(Argument argument)
        {
            if (factoryContext.Arguments.Any(a => a.Name == argument.Name))
            {
                ThrowHelper.ThrowInvalidOperationException($"An argument with the name '{argument.Name}' has already been added to the command.");
            }

            factoryContext.Arguments.Add(argument);
        }
    }

    private sealed class ParameterBinderBuilder(
        ParameterInfo parameter,
        CommandHandlerDelegateFactoryContext factoryContext)
        : BinderBuilder(factoryContext)
        , ICommandHandlerParameterBinderContext
    {
        public ParameterInfo Parameter
            => parameter;
    }

    private abstract class Generic
    {
        private static readonly ConcurrentDictionary<Type, Generic> _factories = new();

        public static Generic ForType(Type type)
            => _factories.GetOrAdd(type, t => (Generic)Activator.CreateInstance(typeof(Generic<>).MakeGenericType(t))!);

        public abstract MethodInfo ValueTaskAsTaskMethod { get; }

        public abstract MethodInfo TaskFromResultMethod { get; }

        public abstract MethodInfo HandleResultMethod { get; }

        public abstract MethodInfo CommandResultExecute { get; }
    }

    private sealed class Generic<T>
        : Generic
        where T : notnull
    {
        public override MethodInfo ValueTaskAsTaskMethod { get; } = typeof(ValueTask<T>).GetMethod(nameof(ValueTask<>.AsTask))!;

        public override MethodInfo TaskFromResultMethod { get; } = GetMethodInfo<Func<T, Task<T>>>(
            static (result) => Task.FromResult(result));

        public override MethodInfo HandleResultMethod { get; } = GetMethodInfo<Func<Task<T>, CommandInvocationContext, CancellationToken, Task>>(
            static (commandResult, invocationContext, cancellationToken) => Handle.HandleHandlerResult(commandResult, invocationContext, cancellationToken));

        public override MethodInfo CommandResultExecute { get; } = GetMethodInfo<Func<Task<T>, CommandInvocationContext, CancellationToken, Task>>(
            static (commandResult, invocationContext, cancellationToken) => Handle.HandleCommandResult(commandResult, invocationContext, cancellationToken));
    }

    private static partial class Handle
    {
        public static async Task Invoke(object? target, Task<object?>[] argumentTasks, Func<object?, object?[], Task> handler)
        {
            var args = await Task.WhenAll(argumentTasks);
            await handler(target, args);
        }

        public static async Task HandleCommandResult<T>(Task<T> commandResult, CommandInvocationContext invocationContext, CancellationToken cancellationToken)
        {
            var result = await commandResult;
            try
            {
                await ((ICommandResult)result!).Execute(invocationContext, cancellationToken);
            }
            finally
            {
                if (result is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else if (result is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public static async Task HandleHandlerResult<T>(Task<T> commandResult, CommandInvocationContext invocationContext, CancellationToken cancellationToken)
        {
            var result = await commandResult;

            try
            {
                var handler = invocationContext.Metadata.OfType<ResultHandlerMetadata>().SingleOrDefault()?.Handler
                    ?? ThrowHelper.ThrowInvalidOperationException<ICommandResultHandler>($"The command {invocationContext.Command.Name} did not have a {nameof(ResultHandlerMetadata)} registered in its metadata.");

                await handler.HandleResult(result, invocationContext, cancellationToken);
            }
            finally
            {
                if (result is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else if (result is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
