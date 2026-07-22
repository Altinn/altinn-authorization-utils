namespace Altinn.Authorization.CommandLine.Logging;

/// <summary>
/// Options for a <see cref="AnsiConsoleLogger"/>.
/// </summary>
public class AnsiConsoleLoggerOptions
{
    /// <summary>
    /// Gets or sets the name of the log message formatter to use.
    /// </summary>
    /// <value>
    /// The default value is <see langword="simple" />.
    /// </value>
    public string? FormatterName { get; set; }
}
