using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class CheckCommandResult
{
    public static CheckCommandResult Full(RepositoryChecker checker, AltinnRepository repository)
        => new CheckCommandResult((reporter, cancellationToken) => checker.Check(repository, reporter, cancellationToken));

    public static CheckCommandResult Partial(Checker checker, AltinnRepository repository, IReadOnlyList<IRepositoryCheck> checks)
        => new CheckCommandResult((reporter, cancellationToken) => checker.Check(repository, checks, reporter, cancellationToken));

    private readonly Func<ICheckReporter?, CancellationToken, Task> _run;

    private CheckCommandResult(Func<ICheckReporter?, CancellationToken, Task> run)
    {
        _run = run;
    }

    public Task Execute(ICheckReporter? reporter, CancellationToken cancellationToken = default)
        => _run(reporter, cancellationToken);
}
