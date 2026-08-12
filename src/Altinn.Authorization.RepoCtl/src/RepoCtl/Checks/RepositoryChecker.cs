using System.Collections.Immutable;
using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class RepositoryChecker
{
    private readonly ImmutableArray<IRepositoryCheck> _checks;
    private readonly Checker _checker;

    public RepositoryChecker(IEnumerable<IRepositoryCheck> checks, Checker checker)
    {
        _checks = [.. checks];
        _checker = checker;
    }

    public Task Check(AltinnRepository repository, CancellationToken cancellationToken = default)
        => _checker.Check(repository, _checks, cancellationToken);

    public Task Check(AltinnRepository repository, ICheckReporter? reporter, CancellationToken cancellationToken = default)
        => _checker.Check(repository, _checks, reporter, cancellationToken);
}
