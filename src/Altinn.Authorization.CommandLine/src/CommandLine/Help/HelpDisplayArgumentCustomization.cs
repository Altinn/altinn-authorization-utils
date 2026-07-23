using System.Collections.Immutable;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Represents customization options for displaying the argument of a command or option in help text.
/// </summary>
public sealed class HelpDisplayArgumentCustomization
{
    /// <summary>
    /// Gets a <see cref="HelpDisplayArgumentCustomization"/> instance that indicates the argument should be hidden in the help text.
    /// </summary>
    public static readonly HelpDisplayArgumentCustomization Hidden = new(shouldDisplay: false, customDisplay: null);

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
        return new(shouldDisplay: false, customDisplay: () => immutable);
    }

    private readonly bool _shouldDisplay;
    private readonly Func<IEnumerable<TextSegment>>? _customDisplay;

    private HelpDisplayArgumentCustomization(bool shouldDisplay, Func<IEnumerable<TextSegment>>? customDisplay)
    {
        _shouldDisplay = shouldDisplay;
        _customDisplay = customDisplay;
    }

    /// <summary>
    /// Whether the argument should be hidden in the help text.
    /// </summary>
    public bool ShouldDisplay
        => _shouldDisplay;

    /// <summary>
    /// Gets a function that returns a custom display for the argument in the help text.
    /// </summary>
    public Func<IEnumerable<TextSegment>>? CustomDisplay
        => _customDisplay;
}
