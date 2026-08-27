using System.CommandLine;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a service that can configure command line options.
/// </summary>
public interface IConfigureOption
{
    /// <summary>
    /// Configures the specified option.
    /// </summary>
    /// <typeparam name="T">The option type.</typeparam>
    /// <param name="option">The option to configure.</param>
    void Configure<T>(Option<T> option);
}
