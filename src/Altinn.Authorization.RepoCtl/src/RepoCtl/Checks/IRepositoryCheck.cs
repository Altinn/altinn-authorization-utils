using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Checks;

internal interface IRepositoryCheck
{
    Task<CheckResult> Check(AltinnRepository repository, CancellationToken cancellationToken);
}
