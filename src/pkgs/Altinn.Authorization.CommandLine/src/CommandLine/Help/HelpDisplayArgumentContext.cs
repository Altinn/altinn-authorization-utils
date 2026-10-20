using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Provides context for displaying an argument in help text.
/// </summary>
/// <param name="DefaultArgumentColor">The default argument color for the current help location.</param>
public sealed record HelpDisplayArgumentContext(Color DefaultArgumentColor);
