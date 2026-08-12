using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Results;
using CommunityToolkit.Diagnostics;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class CheckCommandResultHandlerResolver
    : ICommandResultHandlerResolver
{
    /// <inheritdoc/>
    public bool TryResolve(ICommandResultHandlerResolver resolvers, Type type, [NotNullWhen(true)] out ICommandResultHandler? handler)
    {
        if (type == typeof(CheckCommandResult))
        {
            if (!resolvers.TryResolve(resolvers, typeof(CheckRunResult), out var formatHandler))
            {
                ThrowHelper.ThrowInvalidOperationException($"No formatter registered for {nameof(CheckRunResult)}.");
            }

            handler = new Handler(formatHandler);
            return true;
        }

        handler = null;
        return false;
    }

    private sealed class Handler(ICommandResultHandler formatHandler)
        : ICommandResultHandler
        , ICommandResultBinder
    {
        public void Bind(ICommandHandlerBinderContext context)
        {
            if (formatHandler is ICommandResultBinder binder)
            {
                binder.Bind(context);
            }
        }

        public async Task HandleResult(object? result, CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(result);
            Guard.IsAssignableToType(result, typeof(CheckCommandResult));

            CheckCommandResult checkResult = (CheckCommandResult)result;

            var collector = CheckRunResult.CreateCollector();
            if (context.Console.StdErr.Profile.Capabilities.Interactive)
            {
                await RunChecksWithStatus(checkResult, collector, context.Console.StdErr, cancellationToken);
            }
            else
            {
                await RunChecks(checkResult, collector, cancellationToken);
            }

            await formatHandler.HandleResult(collector.Build(), context, cancellationToken);
        }

        private Task RunChecks(CheckCommandResult result, ICheckReporter reporter, CancellationToken cancellationToken)
            => result.Execute(reporter, cancellationToken);

        private async Task RunChecksWithStatus(CheckCommandResult result, ICheckReporter reporter, IAnsiConsole ansiConsole, CancellationToken cancellationToken)
        {
            await ansiConsole.Status()
                .StartAsync("Running checks...", async ctx =>
                {
                    var statusReporter = new StatusReporter(reporter, ctx);
                    await RunChecks(result, statusReporter, cancellationToken);
                });
        }
    }

    private sealed class StatusReporter
        : ICheckReporter
    {
        private readonly ICheckReporter _inner;
        private readonly StatusContext _context;

        public StatusReporter(ICheckReporter inner, StatusContext context)
        {
            _inner = inner;
            _context = context;
        }

        ValueTask ICheckReporter.RunStarted(IReadOnlyList<IRepositoryCheck> checks, CancellationToken cancellationToken)
            => _inner.RunStarted(checks, cancellationToken);

        ValueTask ICheckReporter.CheckStarted(IRepositoryCheck check, CancellationToken cancellationToken)
        {
            _context.Status = check.CheckDisplayName;

            return _inner.CheckStarted(check, cancellationToken);
        }

        ValueTask ICheckReporter.IssueFound(IRepositoryCheck check, CheckIssue issue, CancellationToken cancellationToken)
            => _inner.IssueFound(check, issue, cancellationToken);

        ValueTask ICheckReporter.CheckCompleted(IRepositoryCheck check, uint issues, CancellationToken cancellationToken)
            => _inner.CheckCompleted(check, issues, cancellationToken);

        ValueTask ICheckReporter.RunCompleted(IReadOnlyList<IRepositoryCheck> checks, uint issues, CancellationToken cancellationToken)
            => _inner.RunCompleted(checks, issues, cancellationToken);
    }
}
