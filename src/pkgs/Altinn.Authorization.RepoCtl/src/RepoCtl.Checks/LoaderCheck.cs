using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Checks;

/// <summary>
/// This "check" simply returns all diagnostics found while loading the repository.
/// </summary>
internal sealed class LoaderCheck
    : IRepositoryCheck
{
    public string CheckId
        => "repoctl-loader";

    public string CheckDisplayName
        => "RepoCtl Repository Loader";

    public IAsyncEnumerable<Diagnostic> Check(AltinnRepository repository, CancellationToken cancellationToken)
        => repository.Diagnostics.ToAsyncEnumerable();
}
