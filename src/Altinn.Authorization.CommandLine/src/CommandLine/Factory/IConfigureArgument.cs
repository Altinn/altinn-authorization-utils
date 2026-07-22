using System.CommandLine;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a service that can configure command line arguments.
/// </summary>
public interface IConfigureArgument
{
    /// <summary>
    /// Configures the specified argument.
    /// </summary>
    /// <typeparam name="T">The argument type.</typeparam>
    /// <param name="argument">The argument to configure.</param>
    void Configure<T>(Argument<T> argument);
}
