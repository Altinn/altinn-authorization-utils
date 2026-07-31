namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Defines a formattable type that can be formatted using a specific formatter.
/// </summary>
/// <typeparam name="T">The type of the formatter.</typeparam>
public interface IFormattable<T>
    where T : IFormat
{
    /// <summary>
    /// Formats the current instance using the specified formatter, formatting context, and cancellation token.
    /// </summary>
    /// <param name="format">The formatter to use.</param>
    /// <param name="writer">The formatting context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public ValueTask Format(T format, IFormatWriter writer, CancellationToken cancellationToken = default);
}
