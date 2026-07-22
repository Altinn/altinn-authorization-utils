using System.CommandLine;
using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Attribute that specifies a parameter should be bound from a command line argument.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public class ArgumentAttribute
    : Attribute
    , IFromArgumentMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
    /// </summary>
    public ArgumentAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the argument to bind from. If null, the parameter name will be used.</param>
    public ArgumentAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// The name of the argument to bind from. If null, the parameter name will be used.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The description of the argument. If null, no description will be used.
    /// </summary>
    public string? Description { get; init; }

    /// <inheritdoc/>
    public virtual Argument CreateArgument(IServiceProvider serviceProvider, Type parameterType, string parameterName, StrongBox<object?>? defaultValueBox)
    {
        var factory = serviceProvider.GetRequiredService<ArgumentFactory>();

        return factory.Create(parameterType, Name ?? parameterName, Description, defaultValueBox);
    }
}
