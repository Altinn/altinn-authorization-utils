using Spectre.Console;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Cli.Utils;

/// <summary>
/// Extensions for <see cref="Task"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TaskExtensions
{
    /// <summary>
    /// Logs a message if the task fails.
    /// </summary>
    /// <typeparam name="T">The task type.</typeparam>
    /// <param name="task">The task.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A wrapped task.</returns>
    public static async Task<T> LogOnFailure<T>(this Task<T> task, string message)
    {
        try
        {
            return await task;
        }
        catch
        {
            AnsiConsole.MarkupLine(message);
            throw;
        }
    }

    /// <summary>
    /// Logs a message if the task fails.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A wrapped task.</returns>
    public static async Task LogOnFailure(this Task task, string message)
    {
        try
        {
            await task;
        }
        catch
        {
            AnsiConsole.MarkupLine(message);
            throw;
        }
    }
}
