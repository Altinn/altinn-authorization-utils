namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Defines a formatter that can format values of a specific type for display in the console.
/// </summary>
public interface IFormat
{
    /// <summary>
    /// Gets the name of the formatter, which is used to identify the formatter in the console output.
    /// </summary>
    /// <remarks>
    /// The name of the formatter should be unique within the context of the application, and should
    /// not contain any whitespace or special characters. Examples of valid formatter names include "json", "simple",
    /// and "rich". The name should be stable between invoking <see cref="Name"/>.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// Formats the specified value for display in the console using the provided formatting context and cancellation token.
    /// </summary>
    /// <typeparam name="T">The type of value to format.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <param name="writer">The formatting context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public ValueTask Write<T>(T value, IFormatWriter writer, CancellationToken cancellationToken = default);
}
