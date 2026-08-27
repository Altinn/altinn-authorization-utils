using System.CommandLine;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Interface marking attributes that specify a parameter should be bound from a command line argument.
/// </summary>
public interface IFromArgumentMetadata
{
    /// <summary>
    /// Creates an <see cref="Argument"/> instance for the specified argument type using the provided service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for creating the argument.</param>
    /// <param name="parameterType">The type of the argument to create.</param>
    /// <param name="parameterName">The name of the parameter to use if the argument name is not specified.</param>
    /// <param name="defaultValueBox">A <see cref="StrongBox{T}"/> containing the default value for the argument, if any.</param>
    /// <returns>An <see cref="Argument"/> instance for the specified argument type.</returns>
    Argument CreateArgument(IServiceProvider serviceProvider, Type parameterType, string parameterName, StrongBox<object?>? defaultValueBox);
}
