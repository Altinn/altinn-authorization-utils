using System.Net;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Defines a set of problems that can occur when working with Altinn repositories.
/// </summary>
public static class Problems
{
    private static readonly ProblemDescriptorFactory _factory
        = ProblemDescriptorFactory.New("REPO");

    /// <summary>
    /// Gets a problem descriptor indicating that the repository root could not be found when attempting to load an Altinn repository.
    /// </summary>
    public static ProblemDescriptor RepositoryRootNotFound { get; }
        = _factory.Create(1, HttpStatusCode.NotFound, "Repository root not found. No '.repo.json/jsonc' file was found in the current directory or any of its parent directories.");

    /// <summary>
    /// Gets a problem descriptor indicating that the repository configuration file could not be deserialized when attempting to load an Altinn repository.
    /// </summary>
    public static ProblemDescriptor RepositoryConfigSerializationFailed { get; }
        = _factory.Create(2, HttpStatusCode.InternalServerError, "Failed to deserialize the repository configuration file.");

    /// <summary>
    /// Gets a problem descriptor indicating that the repository is in 'root' mode but does not specify a 'root-kind' when attempting to load an Altinn repository.
    /// </summary>
    public static ProblemDescriptor RootModeWithoutKind { get; }
        = _factory.Create(3, HttpStatusCode.BadRequest, "The repository is in 'root' but does not specify 'root-kind'.");

    /// <summary>
    /// Gets a problem descriptor indicating that the repository is in 'root' mode but also contains directories for specific kinds when attempting to load an Altinn repository.
    /// </summary>
    public static ProblemDescriptor MixedModeRepository { get; }
        = _factory.Create(4, HttpStatusCode.BadRequest, "Cannot mix 'root' and 'kind' modes. `src` should only contain vertical-kind directories.");

    /// <summary>
    /// Gets a problem descriptor indicating that a vertical has a dependency on another vertical that was not found in the repository.
    /// </summary>
    public static ProblemDescriptor DependencyNotFound { get; }
        = _factory.Create(5, HttpStatusCode.BadRequest, "A vertical has a dependency on another vertical that was not found in the repository.");

    /// <summary>
    /// Gets a problem descriptor indicating that a dependency cycle was detected among the verticals in the repository.
    /// </summary>
    public static ProblemDescriptor DependencyCycle { get; }
        = _factory.Create(6, HttpStatusCode.BadRequest, "A dependency cycle was detected among the verticals in the repository.");

    /// <summary>
    /// Gets a problem descriptor indicating that a vertical has a dependency on another vertical that is not a valid dependency according to the vertical kinds.
    /// </summary>
    public static ProblemDescriptor InvalidDependency { get; }
        = _factory.Create(7, HttpStatusCode.BadRequest, "A vertical has a dependency on another vertical that is not a valid dependency according to the vertical kinds.");

    /// <summary>
    /// Gets a problem descriptor indicating that the repository configuration is invalid.
    /// </summary>
    public static ProblemDescriptor InvalidConfig { get; }
        = _factory.Create(8, HttpStatusCode.BadRequest, "The repository configuration is invalid.");

    /// <summary>
    /// Gets a problem descriptor indicating that the vertical version is invalid.
    /// </summary>
    public static ProblemDescriptor InvalidVersion { get; }
        = _factory.Create(9, HttpStatusCode.BadRequest, "The repository version is invalid.");
}
