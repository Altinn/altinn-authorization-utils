using System.CommandLine;
using System.Runtime.CompilerServices;

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
    /// <param name="defaultValueBox">The box containing the default value for the option, if any.</param>
    /// <returns>The created option.</returns>
    Option CreateOption(IServiceProvider serviceProvider, Type parameterType, string parameterName, StrongBox<object?>? defaultValueBox);
}
