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
    /// Gets or sets customization for displaying the argument in the help text.
    /// </summary>
    public HelpDisplayArgumentCustomization? Argument { get; set; }

    /// <summary>
    /// Gets or sets a function that returns the default value of the argument to be displayed in the help text.
    /// </summary>
    public Func<HelpDisplayArgumentContext, IEnumerable<TextSegment>?>? GetDefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the provider used to display the default when no explicit help override is set.
    /// </summary>
    public DefaultValueProvider? DefaultValueProvider { get; set; }
}
