using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Checks;

/// <summary>
/// Defines a contract for repository checks that can be executed against an Altinn repository.
/// </summary>
public interface IRepositoryCheck
{
    /// <summary>
    /// Stable identifier for the check, used in the json output.
    /// </summary>
    string CheckId { get; }

    /// <summary>
    /// Display name for the check.
    /// </summary>
    string CheckDisplayName { get; }

    /// <summary>
    /// Executes the repository check against the specified Altinn repository.
    /// </summary>
    /// <param name="repository">The Altinn repository to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of diagnostics produced by the check.</returns>
    IAsyncEnumerable<Diagnostic> Check(AltinnRepository repository, CancellationToken cancellationToken);
}
