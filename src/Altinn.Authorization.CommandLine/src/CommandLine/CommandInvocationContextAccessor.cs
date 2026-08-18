using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine;

internal sealed class CommandInvocationContextAccessor
    : ICommandInvocationContextAccessor
{
    private static readonly AsyncLocal<CommandInvocationContextHolder> _httpContextCurrent
        = new();

    /// <inheritdoc/>
    public CommandInvocationContext? CommandInvocationContext
    {
        get
        {
            return _httpContextCurrent.Value?.Context;
        }
        set
        {
            // Clear current HttpContext trapped in the AsyncLocals, as its done.
            _httpContextCurrent.Value?.Context = null;

            if (value != null)
            {
                // Use an object indirection to hold the HttpContext in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                _httpContextCurrent.Value = new CommandInvocationContextHolder { Context = value };
            }
        }
    }

    public static void Configure(ICommandConventionBuilder builder)
    {
        builder.Finally(static builder =>
        {
            builder.Middleware.Insert(0, async (context, next, cancellationToken) =>
            {
                var accessor = context.ApplicationServices.GetRequiredService<ICommandInvocationContextAccessor>();
                accessor.CommandInvocationContext = context;

                try
                {
                    await next(context, cancellationToken);
                }
                finally
                {
                    accessor.CommandInvocationContext = null;
                }
            });
        }, recursive: true);
    }

    private sealed class CommandInvocationContextHolder
    {
        public CommandInvocationContext? Context;
    }
}
