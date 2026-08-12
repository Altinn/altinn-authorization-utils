using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Checks;

internal interface IRepositoryCheck
{
    string CheckId { get; }
    string CheckDisplayName { get; }

    IAsyncEnumerable<CheckIssue> Check(AltinnRepository repository, CancellationToken cancellationToken);
}
