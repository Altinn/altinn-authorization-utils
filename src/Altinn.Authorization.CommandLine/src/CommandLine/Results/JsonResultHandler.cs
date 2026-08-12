using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Formatting.Json;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Results;

internal sealed class JsonResultHandler
    : ICommandResultHandler
    , ICommandResultHandlerResolver
{
    bool ICommandResultHandlerResolver.TryResolve(CommandResultHandlerResolver resolver, Type type, [NotNullWhen(true)] out ICommandResultHandler? handler)
    {
        if (type == typeof(JsonDocument))
        {
            handler = this;
            return true;
        }

        if (type == typeof(JsonElement))
        {
            handler = this;
            return true;
        }

        handler = null;
        return false;
    }

    public Task HandleResult(JsonDocument result, CommandInvocationContext context, CancellationToken cancellationToken = default)
        => HandleResult(result.RootElement, context, cancellationToken);

    public Task HandleResult(JsonElement result, CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        context.Console.Write(JsonWithCommentsText.Create(result));
        return Task.CompletedTask;
    }

    public Task HandleResult(string result, CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        context.Console.Write(JsonWithCommentsText.Create(result));
        return Task.CompletedTask;
    }

    Task ICommandResultHandler.HandleResult(object? result, CommandInvocationContext context, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(result);

        if (result is JsonDocument document)
        {
            return HandleResult(document, context, cancellationToken);
        }

        if (result is JsonElement element)
        {
            return HandleResult(element, context, cancellationToken);
        }

        if (result is string str)
        {
            return HandleResult(str, context, cancellationToken);
        }

        return ThrowHelper.ThrowArgumentException<Task>(nameof(result), $"Unsupported result type: {TypeNameHelper.GetTypeDisplayName(result.GetType(), fullName: false)}");
    }
}
