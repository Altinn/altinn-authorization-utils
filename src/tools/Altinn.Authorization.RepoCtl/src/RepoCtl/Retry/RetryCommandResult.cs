using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Results;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Retry;

internal sealed class RetryCommandResult
    : ICommandResult
{
    private readonly ICommandResult _inner;
    private readonly ushort _maxRetries;
    private readonly Func<int, TimeSpan> _delayFactory;

    public RetryCommandResult(ICommandResult inner, ushort maxRetries, Func<int, TimeSpan> delayFactory)
    {
        Guard.IsNotNull(inner);
        Guard.IsNotNull(delayFactory);
        Guard.IsGreaterThan(maxRetries, 0U);

        _inner = inner;
        _maxRetries = maxRetries;
        _delayFactory = delayFactory;
    }

    public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        CommandInvocationContext child = null!;

        for (var attempt = 0; attempt < _maxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var delay = _delayFactory(attempt);
                context.Console.StdErr.WriteLine();
                context.Console.StdErr.Write(Markup.FromInterpolated($"[gray]# {attempt} failed with return code {child.ReturnCode}. Retrying in {delay.TotalSeconds} seconds...[/]\n"));

                var timeProvider = context.ApplicationServices.GetRequiredService<TimeProvider>();
                await Task.Delay(delay, timeProvider, cancellationToken);
            }

            child = context.CreateChildContext();
            await _inner.Execute(child, cancellationToken);

            if (child.ReturnCode is 0)
            {
                return;
            }
        }

        // if we reach this point, all attempts have failed. Propagate the return code from the last attempt.
        context.ReturnCode = child.ReturnCode;
    }
}
