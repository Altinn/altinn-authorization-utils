using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Provides context for displaying an argument in help text.
/// </summary>
/// <param name="DefaultArgumentColor">The default argument color for the current help location.</param>
/// <param name="DefaultValueProvider">The provider associated with the default being displayed.</param>
public sealed record HelpDisplayArgumentContext(
    Color DefaultArgumentColor,
    DefaultValueProvider? DefaultValueProvider = null);
