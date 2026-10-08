using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Checks;

internal interface ICheckReporter
{
    ValueTask RunStarted(IReadOnlyList<IRepositoryCheck> checks, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask CheckStarted(IRepositoryCheck check, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask DiagnosticFound(IRepositoryCheck check, Diagnostic diag, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask CheckCompleted(IRepositoryCheck check, CheckSummary summary, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask RunCompleted(IReadOnlyList<IRepositoryCheck> checks, CheckSummary summary, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
