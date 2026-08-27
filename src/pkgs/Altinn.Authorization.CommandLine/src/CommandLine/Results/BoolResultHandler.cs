namespace Altinn.Authorization.CommandLine.Results;

internal sealed class BoolResultHandler
    : CommandResultHandler<bool>
{
    protected override Task HandleResult(bool result, CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        context.ReturnCode = result ? 0 : 1;
        return Task.CompletedTask;
    }
}
