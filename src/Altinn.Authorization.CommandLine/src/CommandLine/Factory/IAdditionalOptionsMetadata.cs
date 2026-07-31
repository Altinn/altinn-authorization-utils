using System.CommandLine;

namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a command-metadata that can be used to provide additional options for a command.
/// </summary>
public interface IAdditionalOptionsMetadata
{
    /// <summary>
    /// Gets the additional options that can be used for the command.
    /// </summary>
    public IEnumerable<Option> AdditionalOptions { get; }
}
