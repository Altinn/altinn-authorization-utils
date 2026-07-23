using System.CommandLine;
using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Attribute that specifies a parameter should be bound from a command line option.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public class OptionAttribute
    : Attribute
    , IFromOptionMetadata
{
    private bool? _isRequired;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionAttribute"/> class with the specified name and aliases.
    /// </summary>
    public OptionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionAttribute"/> class with the specified name and aliases.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="aliases">The option aliases.</param>
    public OptionAttribute(string name, params string[] aliases)
    {
        Name = name;
        Aliases = aliases;
    }

    /// <summary>
    /// The name of the option to bind from. If null, the parameter name will be used.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The aliases for the option. If null, no aliases will be used.
    /// </summary>
    public string[]? Aliases { get; }

    /// <summary>
    /// The description of the option. If null, no description will be used.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Indicates whether the option is required. If null, the option will be optional.
    /// </summary>
    public bool IsRequired
    {
        get => _isRequired ?? false;
        set => _isRequired = value;
    }

    /// <inheritdoc/>
    public virtual Option CreateOption(IServiceProvider serviceProvider, Type parameterType, string parameterName, StrongBox<object?>? defaultValueBox)
    {
        var factory = serviceProvider.GetRequiredService<OptionFactory>();

        return factory.Create(parameterType, Name ?? $"--{parameterName}", Aliases ?? [], Description, IsRequired, defaultValueBox);
    }
}
