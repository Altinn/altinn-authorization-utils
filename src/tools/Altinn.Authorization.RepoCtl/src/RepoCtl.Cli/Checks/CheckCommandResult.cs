using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class CheckCommandResult
{
    public static CheckCommandResult Full(RepositoryChecker checker, AltinnRepository repository)
        => new CheckCommandResult(repository, checker.Check);

    public static CheckCommandResult Partial(Checker checker, AltinnRepository repository, IReadOnlyList<IRepositoryCheck> checks)
        => new CheckCommandResult(repository, (repository, reporter, cancellationToken) => checker.Check(repository, checks, reporter, cancellationToken));

    private readonly AltinnRepository _repository;
    private readonly Func<AltinnRepository, ICheckReporter?, CancellationToken, Task> _run;

    private CheckCommandResult(AltinnRepository repository, Func<AltinnRepository, ICheckReporter?, CancellationToken, Task> run)
    {
        _repository = repository;
        _run = run;
    }

    public AltinnRepository Repository => _repository;

    public Task Execute(ICheckReporter? reporter, CancellationToken cancellationToken = default)
        => _run(_repository, reporter, cancellationToken);
}
