namespace Altinn.Authorization.CommandLine.Parsing;

/// <summary>Configures conversion of command-line and default values.</summary>
public sealed class ParsingOptions
{
    /// <summary>Gets parsers tried in list order before built-in conversion.</summary>
    public IList<IValueParser> AdditionalParsers { get; } = new List<IValueParser>();
}
