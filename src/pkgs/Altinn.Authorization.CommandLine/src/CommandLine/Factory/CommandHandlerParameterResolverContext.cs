using System.CommandLine;
using System.Reflection;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Provides state and invocation services for resolving a command handler parameter.
/// </summary>
public abstract class CommandHandlerParameterResolverContext
{
    internal static CommandHandlerParameterResolverContext Create<T>(CommandInvocationContext invocationContext, ParameterInfo parameterInfo, ICommandHandlerParameterResolver parameterResolver)
        => new Typed<T>(invocationContext, parameterInfo, parameterResolver);

    private readonly ICommandHandlerParameterResolver _parameterResolver;
    private readonly List<string> _errors = new();

    /// <summary>
    /// Gets the errors that have been added during parameter resolution.
    /// </summary>
    protected IReadOnlyList<string> Errors
        => _errors;

    /// <summary>
    /// Gets the <see cref="CommandInvocationContext"/> for the current command handler invocation.
    /// </summary>
    public CommandInvocationContext InvocationContext { get; }

    /// <summary>
    /// Gets the parse result for the command invocation.
    /// </summary>
    public ParseResult ParseResult
        => InvocationContext.ParseResult;

    /// <summary>
    /// Gets the service provider for the command invocation.
    /// </summary>
    public IServiceProvider ApplicationServices
        => InvocationContext.ApplicationServices;

    /// <summary>
    /// Gets the parameter for which the value is being resolved.
    /// </summary>
    public ParameterInfo ParameterInfo { get; }

    /// <summary>
    /// Gets the type of the parameter for which the value is being resolved.
    /// </summary>
    public Type ParameterType
        => ParameterInfo.ParameterType;

    private protected CommandHandlerParameterResolverContext(
        CommandInvocationContext invocationContext,
        ParameterInfo parameterInfo,
        ICommandHandlerParameterResolver parameterResolver)
    {
        Guard.IsNotNull(invocationContext);
        Guard.IsNotNull(parameterInfo);

        InvocationContext = invocationContext;
        ParameterInfo = parameterInfo;

        _parameterResolver = parameterResolver;
    }

    /// <summary>
    /// Adds an error encountered while resolving the parameter.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    public void AddError(string errorMessage)
    {
        _errors.Add(errorMessage);
    }

    /// <summary>
    /// Sets the resolved parameter value.
    /// </summary>
    /// <typeparam name="T">The type of the resolved value.</typeparam>
    /// <param name="value">The resolved value.</param>
    /// <exception cref="ArgumentException">Thrown when a non-null value is not assignable to <see cref="ParameterType"/>.</exception>
    public abstract void SetParameterValue<T>(T value);

    internal Task ResolveParameterValue(CancellationToken cancellationToken)
        => _parameterResolver.ResolveParameterValue(this, cancellationToken);

    internal abstract void Populate(
        out object? slot,
        ref Dictionary<ParameterInfo, IReadOnlyList<string>>? errors);

    private sealed class Typed<T>
        : CommandHandlerParameterResolverContext
    {
        private bool _valueSet;
        private T? _value;

        public Typed(CommandInvocationContext invocationContext, ParameterInfo parameterInfo, ICommandHandlerParameterResolver parameterResolver)
            : base(invocationContext, parameterInfo, parameterResolver)
        {
        }

        public override void SetParameterValue<T1>(T1 value)
        {
            if (value is null)
            {
                _value = default;
                _valueSet = true;
                return;
            }

            if (value is not T typedValue)
            {
                ThrowHelper.ThrowArgumentException(nameof(value), $"Value is not of the expected type {TypeNameHelper.GetTypeDisplayName(typeof(T))}.");
                return;
            }

            _value = typedValue;
            _valueSet = true;
        }

        internal override void Populate(
            out object? slot,
            ref Dictionary<ParameterInfo, IReadOnlyList<string>>? errors)
        {
            // Note: we intentionally do not care about errors here, as they are checked later
            slot = _value;

            if (!_valueSet && Errors.Count == 0)
            {
                AddError($"Parameter resolver of type {TypeNameHelper.GetTypeDisplayName(_parameterResolver.GetType())} did not set a value, nor add any errors for parameter {ParameterInfo.Name} of type {TypeNameHelper.GetTypeDisplayName(ParameterType)}.");
            }

            if (Errors.Count > 0)
            {
                errors ??= new();
                errors[ParameterInfo] = Errors;
            }
        }
    }
}
