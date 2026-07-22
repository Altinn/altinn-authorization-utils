namespace Altinn.Authorization.CommandLine.Results;

internal sealed class IntResultHandler
    : ICommandResultHandler<int>
{
    public Task HandleResult(int result, CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        context.ReturnCode = result;
        return Task.CompletedTask;
    }
}
