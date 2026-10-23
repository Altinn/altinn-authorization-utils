using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Help;
using Altinn.Authorization.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine;

/// <summary>Constructs environment defaults when the target type is known at runtime.</summary>
public static class EnvironmentValueDefaultValueProvider
{
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, string, string, DefaultValueProvider>> _factories = new();

    internal static bool TryCreate(IServiceProvider services, Type parameterType, string envVarName, [NotNullWhen(true)] out DefaultValueProvider? envProvider)
    {
        Guard.IsNotNullOrWhiteSpace(envVarName);
        var text = Environment.GetEnvironmentVariable(envVarName);
        if (string.IsNullOrWhiteSpace(text))
        {
            envProvider = null;
            return false;
        }

        envProvider = _factories.GetOrAdd(parameterType, static type => typeof(EnvironmentValueDefaultValueProvider)
            .GetMethod(nameof(CreateTyped), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(type)
            .CreateDelegate<Func<IServiceProvider, string, string, DefaultValueProvider>>())(services, envVarName, text);
        return true;
    }

    private static DefaultValueProvider<T> CreateTyped<T>(IServiceProvider services, string variableName, string text)
        => new EnvironmentValueDefaultValueProvider<T>(variableName, text, services.GetRequiredService<ValueParser>());

    /// <summary>
    /// Gets the help text used as the "default:" value in the help display.
    /// </summary>
    /// <param name="variableName">The environment variable name.</param>
    /// <param name="context">The <see cref="HelpDisplayArgumentContext"/> providing context for the help display.</param>
    /// <returns>An enumerable of <see cref="TextSegment"/> representing the help text for the environment variable default.</returns>
    public static IEnumerable<TextSegment> GetHelpText(string variableName, HelpDisplayArgumentContext context)
        => [new("$", context.DefaultArgumentColor), new(variableName, context.DefaultArgumentColor)];
}

/// <summary>Parses a captured environment value when its default is requested.</summary>
/// <typeparam name="T">The value type.</typeparam>
public sealed class EnvironmentValueDefaultValueProvider<T>
    : DefaultValueProvider<T>
{
    private readonly string[] _values;
    private readonly ValueParser _parser;

    /// <summary>Creates a lazy environment default using a shared value parser.</summary>
    public EnvironmentValueDefaultValueProvider(string variableName, string text, ValueParser parser)
    {
        Guard.IsNotNullOrWhiteSpace(variableName);
        Guard.IsNotNull(text);
        Guard.IsNotNull(parser);
        VariableName = variableName;
        _values = [text];
        _parser = parser;
    }

    /// <summary>Gets the environment variable name.</summary>
    public string VariableName { get; }

    /// <inheritdoc />
    public override bool TryProvideValue(DefaultValueProviderContext context, out T value, [NotNullWhen(false)] out string? error)
    {
        if (_parser.TryParse(_values, out value, out error))
        {
            return true;
        }

        error = $"Invalid {VariableName} default for {context.Name}: {error}";
        return false;
    }

    /// <inheritdoc />
    public override IEnumerable<TextSegment> GetHelpText(HelpDisplayArgumentContext context)
        => EnvironmentValueDefaultValueProvider.GetHelpText(VariableName, context);
}
