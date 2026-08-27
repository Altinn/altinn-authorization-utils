using System.Collections.Immutable;

namespace Altinn.Authorization.CommandLine.Arguments;

/// <summary>
/// An attribute that indicates that a specific enum value has a custom string representation for command line arguments/options.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ArgumentValuesAttribute
    : Attribute
{
    private readonly ImmutableArray<string> _names;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentValuesAttribute"/> class with the specified string representation.
    /// </summary>
    /// <param name="value">The custom string representation of the enum value.</param>
    /// <param name="aliases">The aliases for the custom string representation of the enum value.</param>
    public ArgumentValuesAttribute(string value, params string[] aliases)
    {
        CanonicalName = value;
        _names = [value, .. aliases];
    }

    /// <summary>
    /// Gets the custom string representation of the enum value.
    /// </summary>
    public string CanonicalName { get; }

    /// <summary>
    /// Get the custom string representations of the enum value, including the canonical name and any aliases.
    /// </summary>
    public ReadOnlySpan<string> Names
        => _names.AsSpan();
}
