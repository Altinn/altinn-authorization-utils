namespace Altinn.Authorization.RepoCtl.Checks;

internal interface ICheckReporter
{
    ValueTask RunStarted(IReadOnlyList<IRepositoryCheck> checks, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask CheckStarted(IRepositoryCheck check, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IssueFound(IRepositoryCheck check, CheckIssue issue, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask CheckCompleted(IRepositoryCheck check, uint issues, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask RunCompleted(IReadOnlyList<IRepositoryCheck> checks, uint issues, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
