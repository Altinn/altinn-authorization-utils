using System.CommandLine;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Interface marking attributes that specify a parameter should be bound from a command line option.
/// </summary>
public interface IFromOptionMetadata
{
    /// <summary>
    /// Creates an option for the specified parameter.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="defaultValueProvider">The default-value provider, or null when no default is configured.</param>
    /// <returns>The created option.</returns>
    Option CreateOption(IServiceProvider serviceProvider, Type parameterType, string parameterName, DefaultValueProvider? defaultValueProvider);
}
