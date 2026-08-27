namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Defines a formattable type that can be formatted using a specific formatter.
/// </summary>
/// <typeparam name="TFormat">The type of the formatter.</typeparam>
public interface IFormatter<TFormat>
    where TFormat : IFormat
{
    /// <summary>
    /// Gets a value indicating whether the formatter can format values of the specified type.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns><see langword="true"/> if the formatter can format values of the specified type; otherwise, <see langword="false"/>.</returns>
    public bool CanFormat(Type type);

    /// <summary>
    /// Formats the current instance using the specified formatter, formatting context, and cancellation token.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="format">The formatter to use.</param>
    /// <param name="writer">The formatting context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public ValueTask Format(object value, TFormat format, IFormatWriter writer, CancellationToken cancellationToken = default);
}
