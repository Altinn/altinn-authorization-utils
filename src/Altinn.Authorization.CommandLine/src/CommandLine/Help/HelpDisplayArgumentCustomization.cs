using System.Collections.Immutable;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Represents customization options for displaying the argument of a command or option in help text.
/// </summary>
public sealed class HelpDisplayArgumentCustomization
{
    private static readonly HelpDisplayArgumentCustomization DisplayDefault = new(hidden: false, customDisplay: null);
    private static readonly HelpDisplayArgumentCustomization DisplayHidden = new(hidden: true, customDisplay: null);

    /// <summary>
    /// Creates a new instance of <see cref="HelpDisplayArgumentCustomization"/> with the specified hidden state and custom display function.
    /// </summary>
    /// <param name="customDisplay">The custom display for the argument in the help text.</param>
    /// <returns>A new instance of <see cref="HelpDisplayArgumentCustomization"/>.</returns>
    public static HelpDisplayArgumentCustomization Create(IEnumerable<string> customDisplay)
    {
        var items = ImmutableArray.CreateBuilder<TextSegment>();
        items.Add("<");
        var first = true;
        foreach (var item in customDisplay)
        {
            if (!first)
            {
                items.Add("|");
            }

            first = false;
            items.Add(new(item, Color.Magenta));
        }
        items.Add(">");

        var immutable = items.ToImmutable();
        return new(hidden: false, customDisplay: () => immutable);
    }

    private readonly bool _hidden;
    private readonly Func<IEnumerable<TextSegment>>? _customDisplay;

    private HelpDisplayArgumentCustomization(bool hidden, Func<IEnumerable<TextSegment>>? customDisplay)
    {
        _hidden = hidden;
        _customDisplay = customDisplay;
    }

    /// <summary>
    /// Whether the argument should be hidden in the help text.
    /// </summary>
    public bool Hidden
        => _hidden;

    /// <summary>
    /// Gets a function that returns a custom display for the argument in the help text.
    /// </summary>
    public Func<IEnumerable<TextSegment>>? CustomDisplay
        => _customDisplay;

    /// <summary>
    /// Creates an implicit conversion from a boolean to a <see cref="HelpDisplayArgumentCustomization"/>.
    /// </summary>
    /// <param name="display">Whether or not the argument should be hidden in the help text.</param>
    public static implicit operator HelpDisplayArgumentCustomization(bool display)
        => !display ? DisplayHidden : DisplayDefault;
}
