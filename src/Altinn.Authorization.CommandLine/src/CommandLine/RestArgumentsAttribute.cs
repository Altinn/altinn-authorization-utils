using System.CommandLine;
using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Attribute that specifies a parameter should be bound from the rest of the command line arguments.
/// </summary>
/// <remarks>
/// This also allows passing nested options after <c>--</c> to avoid ambiguity with the command line parser.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class RestArgumentsAttribute
    : Attribute
    , IFromArgumentMetadata
{
    /// <inheritdoc/>
    public Argument CreateArgument(IServiceProvider serviceProvider, Type parameterType, string parameterName, StrongBox<object?>? defaultValueBox)
    {
        if (parameterType != typeof(string[]))
        {
            throw new InvalidOperationException($"The parameter '{parameterName}' is marked with the {nameof(RestArgumentsAttribute)} but is not of type string[].");
        }

        var factory = serviceProvider.GetRequiredService<ArgumentFactory>();

        var arg = factory.Create(parameterType, "rest", "Rest arguments", new(Array.Empty<string>()));
        arg.Arity = ArgumentArity.ZeroOrMore;

        return arg;
    }
}
