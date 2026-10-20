using System.Collections.Immutable;
using System.CommandLine;

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
    /// <param name="arity">The arity to display instead of the parser arity.</param>
    /// <returns>A new instance of <see cref="HelpDisplayArgumentCustomization"/>.</returns>
    public static HelpDisplayArgumentCustomization Create(IEnumerable<string> customDisplay, ArgumentArity? arity = null)
    {
        var values = customDisplay.ToImmutableArray();
        return new(shouldDisplay: true, customDisplay: context => BuildDisplay(values, context)) { Arity = arity };
    }

    private static IEnumerable<TextSegment> BuildDisplay(ImmutableArray<string> values, HelpDisplayArgumentContext context)
    {
        var items = ImmutableArray.CreateBuilder<TextSegment>();
        items.Add("<");
        var first = true;
        foreach (var item in values)
        {
            if (!first)
            {
                items.Add("|");
            }

            first = false;
            items.Add(new(item, context.DefaultArgumentColor));
        }
        items.Add(">");

        return items.ToImmutable();
    }

    /// <summary>
    /// Gets the arity to display instead of the parser arity.
    /// </summary>
    public ArgumentArity? Arity { get; init; }

    private readonly bool _shouldDisplay;
    private readonly Func<HelpDisplayArgumentContext, IEnumerable<TextSegment>>? _customDisplay;

    private HelpDisplayArgumentCustomization(bool shouldDisplay, Func<HelpDisplayArgumentContext, IEnumerable<TextSegment>>? customDisplay)
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
    public Func<HelpDisplayArgumentContext, IEnumerable<TextSegment>>? CustomDisplay
        => _customDisplay;
}
