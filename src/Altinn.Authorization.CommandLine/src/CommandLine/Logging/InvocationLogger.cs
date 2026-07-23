using System.CommandLine.Parsing;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.CommandLine.Logging;

internal sealed partial class InvocationLogger(ILogger<InvocationLogger> logger, TimeProvider timeProvider)
{
    public void Configure(ICommandConventionBuilder builder)
    {
        builder.Add(builder =>
        {
            var commandName = builder.Name;
            builder.Middleware.Add(async (context, next, cancellationToken) =>
            {
                using var activity = CommandTelemetry.Source.StartActivity(ActivityKind.Internal, name: $"invoke {commandName}", tags: [new("command.name", commandName)]);
                Log.InvokingCommand(logger, commandName);
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    LogCommandInfoFull(logger, context);
                }

                long startTime = timeProvider.GetTimestamp();

                // note: we do not log exceptions here, as those are handled upstream
                try
                {
                    await next(context, cancellationToken);
                }
                finally
                {
                    var duration = timeProvider.GetElapsedTime(startTime);
                    Log.CommandInvoked(logger, commandName, duration, context.ReturnCode);
                }
            });
        }, recursive: true);
    }

    private static void LogCommandInfoFull(ILogger logger, CommandInvocationContext context)
    {
        foreach (var symbol in context.ParseResult.Tokens)
        {
            Log.Token(logger, symbol);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Invoking command '{CommandName}'")]
        public static partial void InvokingCommand(ILogger logger, string commandName);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Command '{CommandName}' invoked in {Duration} with return code {ReturnCode}")]
        public static partial void CommandInvoked(ILogger logger, string commandName, TimeSpan duration, int returnCode);

        [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "{TokenType} token: {TokenValue}")]
        private static partial void Token(ILogger logger, TokenType tokenType, string tokenValue);
        public static void Token(ILogger logger, Token token) => Token(logger, token.Type, token.Value);
    }
}
