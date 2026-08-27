namespace Altinn.Authorization.CommandLine.Formatting;

internal sealed class SelfFormatter<TFormat>
    : IFormatter<TFormat>
    where TFormat : IFormat
{
    /// <summary>
    /// Gets a singleton instance of the <see cref="SelfFormatter{TFormat}"/> class.
    /// </summary>
    public static readonly IFormatter<TFormat> Instance = new SelfFormatter<TFormat>();

    private SelfFormatter()
    {
    }

    public bool CanFormat(Type type)
        => typeof(IFormattable<TFormat>).IsAssignableFrom(type);

    public ValueTask Format(object value, TFormat format, IFormatWriter writer, CancellationToken cancellationToken = default)
        => ((IFormattable<TFormat>)value).Format(format, writer, cancellationToken);
}
