namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Represents customization options for help text generation for a command or option.
/// </summary>
public sealed class HelpCustomization
{
    internal HelpCustomization()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the argument should be displayed in the help text.
    /// </summary>
    public HelpDisplayArgumentCustomization? DisplayArgument { get; set; }

    /// <summary>
    /// Gets or sets a function that returns the default value of the argument to be displayed in the help text.
    /// </summary>
    public Func<IEnumerable<TextSegment>?>? GetDefaultValue { get; set; }
}
