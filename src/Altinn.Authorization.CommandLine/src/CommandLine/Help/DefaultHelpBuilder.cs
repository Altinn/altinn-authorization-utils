using System.Collections;
using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Altinn.Authorization.CommandLine.Utils;
using CommunityToolkit.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Default implementation of <see cref="IHelpBuilder"/> that builds help text for commands.
/// </summary>
public class DefaultHelpBuilder
    : IHelpBuilder
{
    private static Lazy<FigletFont> _slant = new(
        () =>
        {
            var assembly = typeof(DefaultHelpBuilder).Assembly;
            var resourceName = "Altinn.Authorization.CommandLine.slant.flf";

            using var stream = assembly.GetManifestResourceStream(resourceName);

            Debug.Assert(stream is not null);
            return FigletFont.Load(stream);
        },
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <inheritdoc />
    public IRenderable Build(HelpContext context)
    {
        if (context.Command.Hidden)
        {
            return Text.Empty;
        }

        List<IRenderable> sections = GetSections(context)
            .NotNull()
            .Intersperse(Text.NewLine)
            .ToList();

        return sections switch
        {
            [] => Text.Empty,
            [var single] => single,
            _ => new Rows(sections),
        };
    }

    /// <summary>
    /// Gets the font to use for the title section.
    /// </summary>
    protected virtual FigletFont TitleFont
        => _slant.Value;

    /// <summary>
    /// Gets the sections to render for the help text.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>An enumerable of <see cref="IRenderable"/> sections.</returns>
    protected virtual IEnumerable<IRenderable?> GetSections(HelpContext context)
    {
        yield return BuildTitleSection(context);
        yield return BuildSynopsisSection(context);
        yield return BuildCommandUsageSection(context);
        yield return BuildCommandArgumentsSection(context);
        yield return BuildOptionsSection(context);
        yield return BuildAdditionalOptionsSection(context);
        yield return BuildSubcommandsSection(context);
    }

    /// <summary>
    /// Builds the title section of the help text.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>The title section as an <see cref="IRenderable"/> or null if not applicable.</returns>
    protected virtual IRenderable? BuildTitleSection(HelpContext context)
    {
        RootCommand? rootCmd = context.CommandResult.InvocationPath()
            .OfType<RootCommand>()
            .FirstOrDefault();

        if (rootCmd is not { Name: { Length: > 0 } name })
        {
            return null;
        }

        return new FigletText(TitleFont, name)
        {
            Color = Color.Gold1,
        };
    }

    /// <summary>
    /// Builds the synopsis section of the help text.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>The synopsis section as an <see cref="IRenderable"/> or null if not applicable.</returns>
    protected virtual IRenderable? BuildSynopsisSection(HelpContext context)
    {
        return Section.Optional("DESCRIPTION", context.Command.Description);
    }

    /// <summary>
    /// Builds the command usage section of the help text.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>The command usage section as an <see cref="IRenderable"/> or null if not applicable.</returns>
    protected virtual IRenderable? BuildCommandUsageSection(HelpContext context)
    {
        return Section.Optional("USAGE", GetUsage(context.CommandResult));
    }

    /// <summary>
    /// Builds the command arguments section of the help text.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>The command arguments section as an <see cref="IRenderable"/> or null if not applicable.</returns>
    protected virtual IRenderable? BuildCommandArgumentsSection(HelpContext context)
    {
        return Section.Optional("ARGUMENTS", GetArguments(context.CommandResult));
    }

    /// <summary>
    /// Builds the options section of the help text.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>The options section as an <see cref="IRenderable"/> or null if not applicable.</returns>
    protected virtual IRenderable? BuildOptionsSection(HelpContext context)
    {
        return Section.Optional("OPTIONS", GetOptions(context.CommandResult, target: OptionsTarget.InstanceOptions));
    }

    /// <summary>
    /// Builds the options section of the help text.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>The options section as an <see cref="IRenderable"/> or null if not applicable.</returns>
    protected virtual IRenderable? BuildAdditionalOptionsSection(HelpContext context)
    {
        return Section.Optional("ADDITIONAL OPTIONS", GetOptions(context.CommandResult, target: OptionsTarget.RecursiveOptions));
    }

    /// <summary>
    /// Builds the subcommands section of the help text.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>The subcommands section as an <see cref="IRenderable"/> or null if not applicable.</returns>
    protected virtual IRenderable? BuildSubcommandsSection(HelpContext context)
    {
        return Section.Optional("COMMANDS", GetSubcommands(context.Command));
    }

    /// <summary>
    /// Gets the usage information for the specified command.
    /// </summary>
    /// <param name="commandResult">The command result.</param>
    /// <returns>The usage information as an <see cref="IRenderable"/> or null if not applicable.</returns>
    protected virtual IRenderable? GetUsage(CommandResult commandResult)
    {
        Paragraph usage = new();
        var parts = GetUsageParts(commandResult);

        if (!parts.MoveNext())
        {
            return null;
        }

        usage.Append(parts.Current.Text, parts.Current.Style);

        while (parts.MoveNext())
        {
            usage.Append(parts.Current.Text, parts.Current.Style);
        }

        return usage;

        static IEnumerator<(string Text, Style? Style)> GetUsageParts(CommandResult commandResult)
        {
            bool displayOptionTitle = false;

            IEnumerable<Command> parentCommands =
                commandResult.InvocationPath().Reverse();

            var first = true;
            foreach (var parentCommand in parentCommands)
            {
                if (!first)
                {
                    yield return (" ", null);
                }

                first = false;
                if (!displayOptionTitle)
                {
                    displayOptionTitle = parentCommand.Options.Any(x => x.Recursive && !x.Hidden);
                }

                var parentCommandName = (parentCommand is RootCommand root ? root.HelpName : null) ?? parentCommand.Name;
                yield return (parentCommandName, null);

                if (parentCommand.Arguments.Any())
                {
                    using var inner = FormatArgumentUsage(parentCommand.Arguments);

                    while (inner.MoveNext())
                    {
                        yield return inner.Current;
                    }
                }
            }

            var command = commandResult.Command;
            var hasCommandWithHelp = command.Subcommands.Any(x => !x.Hidden);

            if (hasCommandWithHelp)
            {
                yield return (" [", null);
                yield return ("command", Color.Magenta);
                yield return ("]", null);
            }

            displayOptionTitle = displayOptionTitle || command.Options.Any(x => !x.Hidden);

            if (displayOptionTitle)
            {
                yield return (" [", null);
                yield return ("options", Color.Magenta);
                yield return ("]", null);
            }

            if (!command.TreatUnmatchedTokensAsErrors)
            {
                yield return (" [[--] <additional arguments>...]", null);
            }
        }

        static IEnumerator<(string Text, Style? Style)> FormatArgumentUsage(IEnumerable<Argument> arguments)
        {
            foreach (var arg in arguments.Where(static arg => !arg.Hidden))
            {
                yield return (" ", null);

                var isOptional = arg.Arity.MinimumNumberOfValues == 0;
                if (isOptional)
                {
                    yield return ("[", null);
                }

                yield return ("<", null);
                yield return (arg.Name, Color.Cyan);
                yield return (">", null);

                if (arg.Arity.MaximumNumberOfValues > 1)
                {
                    yield return ("...", null);
                }

                if (isOptional)
                {
                    yield return ("]", null);
                }
            }
        }
    }

    /// <summary>
    /// Gets the arguments for the specified command, including those of its parent commands.
    /// </summary>
    /// <param name="commandResult">The command for which to get arguments.</param>
    /// <returns>An IRenderable representing the arguments, or null if there are none.</returns>
    protected virtual IRenderable? GetArguments(CommandResult commandResult)
    {
        var arguments = commandResult.InvocationPath()
            .SelectMany(x => x.Arguments.Where(arg => !arg.Hidden))
            .DistinctBy(arg => arg.Name)
            .ToList();

        if (arguments.Count == 0)
        {
            return null;
        }

        var anyHasDefault = arguments.Any(static arg => arg.HasDefaultValue);

        var grid = new Grid();
        grid.AddColumn(new GridColumn { Padding = new Padding(left: 0, right: 4, top: 0, bottom: 0), NoWrap = true }); // name column

        if (anyHasDefault)
        {
            grid.AddColumn(new GridColumn { Padding = new Padding(left: 0, top: 0, right: 4, bottom: 0) }); // default value column
        }

        grid.AddColumn(new GridColumn { Padding = new Padding(0, 0) }); // description column

        foreach (var arg in arguments)
        {
            var nameCell = GetArgumentUsageLabel(arg);
            var defaultCell = GetArgumentDefaultValueLabel(arg);
            var descriptionCell = GetArgumentDescriptionLabel(arg);

            if (anyHasDefault)
            {
                grid.AddRow(nameCell, defaultCell, descriptionCell);
            }
            else
            {
                grid.AddRow(nameCell, descriptionCell);
            }
        }

        return grid;
    }

    /// <summary>
    /// Gets the subcommands for the specified command.
    /// </summary>
    /// <param name="commandResult">The command result for which to get subcommands.</param>
    /// <param name="target">Specifies whether to include subcommands from the current command, parent commands, or both.</param>
    /// <returns>An IRenderable representing the subcommands, or null if there are none.</returns>
    protected virtual IRenderable? GetOptions(CommandResult commandResult, OptionsTarget target)
    {
        var command = commandResult.Command;

        List<Option> options = new();
        if (target.HasFlag(OptionsTarget.InstanceOptions))
        {
            options.AddRange(command.Options
                .Where(static o => !o.Hidden && !o.Recursive && o is not HelpOption)
                .OrderBy(static o => o is VersionOption));
        }

        if (target.HasFlag(OptionsTarget.RecursiveOptions))
        {
            if (!options.Any(static o => o is HelpOption))
            {
                options.Insert(0, new HelpOption());
            }

            options.AddRange(command.Options
                .Where(static o => !o.Hidden && o.Recursive && o is not HelpOption)
                .OrderBy(static o => o is VersionOption));

            foreach (var current in commandResult.InvocationPath())
            {
                if (current.Options.Any())
                {
                    foreach (var option in current.Options)
                    {
                        // global help aliases may be duplicated, we just ignore them
                        if (option is { Recursive: true, Hidden: false } and not HelpOption
                            && !options.Any(o => o.Name == option.Name))
                        {
                            options.Add(option);
                        }
                    }
                }
            }
        }

        if (options.Count == 0)
        {
            return null;
        }

        var anyHasDefault = options.Any(static arg => arg.HasDefaultValue);

        var grid = new Grid();
        grid.AddColumn(new GridColumn { Padding = new Padding(left: 0, right: 4, top: 0, bottom: 0), NoWrap = true }); // name column

        if (anyHasDefault)
        {
            grid.AddColumn(new GridColumn { Padding = new Padding(left: 0, top: 0, right: 4, bottom: 0) }); // default value column
        }

        grid.AddColumn(new GridColumn { Padding = new Padding(0, 0) }); // description column

        foreach (var opt in options)
        {
            var nameCell = GetOptionUsageLabel(opt);
            var defaultCell = GetOptionDefaultValueLabel(opt);
            var descriptionCell = GetOptionDescriptionLabel(opt);

            if (anyHasDefault)
            {
                grid.AddRow(nameCell, defaultCell, descriptionCell);
            }
            else
            {
                grid.AddRow(nameCell, descriptionCell);
            }
        }

        return grid;
    }

    /// <summary>
    /// Gets the subcommands for the specified command.
    /// </summary>
    /// <param name="command">The command for which to get subcommands.</param>
    /// <returns>An IRenderable representing the subcommands, or null if there are none.</returns>
    protected virtual IRenderable? GetSubcommands(Command command)
    {
        var subcommands = command.Subcommands.Where(x => !x.Hidden).ToList(); //.Select(x => GetTwoColumnRow(x, context)).ToArray();
        if (subcommands.Count == 0)
        {
            return null;
        }

        var grid = new Grid();
        grid.AddColumn(new GridColumn { Padding = new Padding(left: 0, right: 4, top: 0, bottom: 0), NoWrap = true }); // name column
        grid.AddColumn(new GridColumn { Padding = new Padding(0, 0) }); // description column

        foreach (var subcommand in subcommands)
        {
            var nameCell = GetCommandUsageLabel(subcommand);
            var descriptionCell = GetCommandDescriptionLabel(subcommand);

            grid.AddRow(nameCell, descriptionCell);
        }

        return grid;
    }

    /// <summary>
    /// Gets the usage label for the specified command, which is used in the subcommands section of the help text.
    /// </summary>
    /// <param name="command">The command for which to get the usage label.</param>
    /// <returns>An IRenderable representing the usage label for the command.</returns>
    protected virtual IRenderable GetCommandUsageLabel(Command command)
        => GetIdentifierSymbolUsageLabel(command, command.Aliases);

    /// <summary>
    /// Gets the usage label for the specified option, which is used in the options section of the help text.
    /// </summary>
    /// <param name="option">The option for which to get the usage label.</param>
    /// <returns>An IRenderable representing the usage label for the option.</returns>
    protected virtual IRenderable GetOptionUsageLabel(Option option)
        => GetIdentifierSymbolUsageLabel(option, option.Aliases);

    /// <summary>
    /// Gets the usage label for the specified argument, which is used in the arguments section of the help text.
    /// </summary>
    /// <param name="argument">The argument for which to get the usage label.</param>
    /// <returns>An IRenderable representing the usage label for the argument.</returns>
    protected virtual IRenderable GetArgumentUsageLabel(Argument argument)
    {
        Paragraph usage = new();

        foreach (var segment in GetArgumentUsageLabel((Symbol)argument))
        {
            usage.Append(segment.Text, segment.Style ?? Color.Cyan);
        }

        return usage;
    }

    /// <summary>
    /// Gets the description label for the specified command, which is used in the subcommands section of the help text.
    /// </summary>
    /// <param name="command">The command for which to get the description label.</param>
    /// <returns>An IRenderable representing the description label for the command.</returns>
    protected virtual IRenderable GetCommandDescriptionLabel(Command command)
        => new Text(command.Description ?? string.Empty);

    /// <summary>
    /// Gets the description label for the specified option, which is used in the options section of the help text.
    /// </summary>
    /// <param name="option">The option for which to get the description label.</param>
    /// <returns>An IRenderable representing the description label for the option.</returns>
    protected virtual IRenderable GetOptionDescriptionLabel(Option option)
        => new Text(option.Description ?? string.Empty);

    /// <summary>
    /// Gets the description label for the specified argument, which is used in the arguments section of the help text.
    /// </summary>
    /// <param name="argument">The argument for which to get the description label.</param>
    /// <returns>An IRenderable representing the description label for the argument.</returns>
    protected virtual IRenderable GetArgumentDescriptionLabel(Argument argument)
        => new Text(argument.Description ?? string.Empty);

    /// <summary>
    /// Gets the default value label for the specified option, which is used in the options section of the help text.
    /// </summary>
    /// <param name="option">The option for which to get the default value label.</param>
    /// <returns>An IRenderable representing the default value label for the option.</returns>
    protected virtual IRenderable GetOptionDefaultValueLabel(Option option)
        => GetSymbolDefaultValueLabel(option);

    /// <summary>
    /// Gets the default value label for the specified argument, which is used in the arguments section of the help text.
    /// </summary>
    /// <param name="argument">The argument for which to get the default value label.</param>
    /// <returns>An IRenderable representing the default value label for the argument.</returns>
    protected virtual IRenderable GetArgumentDefaultValueLabel(Argument argument)
        => GetSymbolDefaultValueLabel(argument);

    /// <summary>
    /// Gets the description label for the specified symbol.
    /// </summary>
    /// <param name="symbol">The symbol for which to get the usage label.</param>
    /// <param name="aliasSet">A collection of aliases for the symbol.</param>
    /// <returns>An IRenderable representing the usage label for the symbol.</returns>
    protected virtual IRenderable GetIdentifierSymbolUsageLabel(Symbol symbol, ICollection<string>? aliasSet)
    {
        Paragraph usage = new();

        var aliases = aliasSet is null
            ? [symbol.Name]
            : new[] { symbol.Name }.Concat(aliasSet)
                .Select(r => r.SplitPrefix())
                .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
                .GroupBy(t => t.Alias)
                .Select(t => t.First())
                .Select(t => $"{t.Prefix}{t.Alias}");

        var first = true;
        foreach (var alias in aliases)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                usage.Append(", ");
            }

            usage.Append(alias, Color.Cyan); // TODO: Add style for aliases
        }

        foreach (var argument in symbol.GetParameters().Where(static arg => !arg.Hidden))
        {
            if (argument.TryGetHelpCustomization(out var customization)
                && customization.Argument?.ShouldDisplay is false)
            {
                continue;
            }

            IEnumerator<TextSegment> argumentUsage;
            if (customization?.Argument?.CustomDisplay is { } customDisplay)
            {
                argumentUsage = customDisplay().GetEnumerator();
            }
            else
            {
                argumentUsage = GetArgumentUsageLabel(argument).GetEnumerator();
            }

            if (!argumentUsage.MoveNext())
            {
                continue;
            }

            usage.Append(" ");

            do
            {
                usage.Append(argumentUsage.Current.Text, argumentUsage.Current.Style);
            }
            while (argumentUsage.MoveNext());
        }

        if (symbol is Option { Required: true })
        {
            usage.Append(" ");
            usage.Append("(required)");
        }

        return usage;
    }

    /// <summary>
    /// Gets the default value for the specified argument of a symbol.
    /// </summary>
    /// <param name="symbol">The symbol for which to get the default value label.</param>
    /// <returns>An IRenderable representing the default value label for the symbol.</returns>
    protected virtual IRenderable GetSymbolDefaultValueLabel(Symbol symbol)
    {
        var arguments = symbol.GetParameters();
        var defaultArguments = arguments.Where(x => !x.Hidden && ShouldShowDefaultValue(x)).ToList();

        if (defaultArguments.Count == 0)
        {
            return Text.Empty;
        }

        var isSingleArgument = defaultArguments.Count == 1;
        Paragraph result = new();
        result.Append("[");

        var first = true;
        IEnumerable<IEnumerable<TextSegment>> values = defaultArguments
            .Select(arg => GetArgumentDefaultValue(symbol, arg, isSingleArgument).ToList())
            .Where(defaultValue => defaultValue.Count > 0);
        foreach (var defaultValue in values)
        {
            if (!first)
            {
                result.Append(", ");
            }

            first = false;
            foreach (var segment in defaultValue)
            {
                result.Append(segment.Text, segment.Style);
            }
        }

        if (first)
        {
            return Text.Empty;
        }

        result.Append("]");
        return result;
    }

    /// <summary>
    /// Gets the description label for the specified command, which is used in the subcommands section of the help text.
    /// </summary>
    /// <param name="parameter">The symbol representing the parameter for which to get the usage label.</param>
    /// <returns>An enumerable of text segments representing the usage label for the parameter.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected IEnumerable<TextSegment> GetArgumentUsageLabel(Symbol parameter)
    {
        // By default Option.Name == Argument.Name, don't repeat it
        return parameter switch
        {
            Argument argument => GetUsageLabel(argument.HelpName, argument.ValueType, argument.CompletionSources, argument, argument.Arity) ?? [$"<{argument.Name}>"],
            Option option => GetUsageLabel(option.HelpName, option.ValueType, option.CompletionSources, option, option.Arity) ?? [],
            _ => ThrowHelper.ThrowInvalidOperationException<IEnumerable<TextSegment>>($"Unexpected symbol type: {parameter.GetType().Name}"),
        };

        static IEnumerable<TextSegment>? GetUsageLabel(
            string? helpName,
            Type valueType,
            List<Func<CompletionContext, IEnumerable<CompletionItem>>> completionSources,
            Symbol symbol,
            ArgumentArity arity)
        {
            if (!string.IsNullOrWhiteSpace(helpName))
            {
                return [$"<{helpName}>"];
            }

            if (valueType == typeof(bool) ||
                valueType == typeof(bool?) ||
                arity.MaximumNumberOfValues <= 0) // allowing zero arguments means we don't need to show usage
            {
                return null;
            }

            if (completionSources.Count <= 0)
            {
                if (symbol is Option)
                {
                    return [$"<{symbol.Name.TrimStart('-', '/')}>"];
                }

                return null;
            }

            var completions = symbol
                .GetCompletions(CompletionContext.Empty)
                .Select(item => item.Label)
                .ToList();

            if (completions.Count > 0)
            {
                return GetCompletionUsage(completions);
            }

            return null;
        }

        static IEnumerable<TextSegment> GetCompletionUsage(IEnumerable<string> completions)
        {
            var first = true;

            yield return "<";
            foreach (var completion in completions)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    yield return "|";
                }

                yield return completion;
            }

            yield return ">";
        }
    }

    /// <summary>
    /// Gets the default value for the specified argument of a symbol.
    /// </summary>
    /// <param name="parent">The parent command or option.</param>
    /// <param name="parameter">The argument symbol.</param>
    /// <param name="displayArgumentName">Whether to display the argument name in the default value string.</param>
    /// <returns>The formatted default value string.</returns>
    protected virtual IReadOnlyList<TextSegment> GetArgumentDefaultValue(Symbol parent, Symbol parameter, bool displayArgumentName)
    {
        IReadOnlyList<TextSegment>? displayedDefaultValue = null;

        if (parent.TryGetHelpCustomization(out var customization)
            && customization.GetDefaultValue?.Invoke() is { } parentDefaultValue)
        {
            displayedDefaultValue = parentDefaultValue.ToList();
        }
        else if (parameter.TryGetHelpCustomization(out customization)
            && customization.GetDefaultValue?.Invoke() is { } ownDefaultValue)
        {
            displayedDefaultValue = ownDefaultValue.ToList();
        }

        displayedDefaultValue ??= GetArgumentDefaultValue(parameter).ToList();

        if (displayedDefaultValue is null || displayedDefaultValue.All(static x => string.IsNullOrWhiteSpace(x.Text)))
        {
            return [];
        }

        string label = displayArgumentName
            ? "default"
            : parameter.Name;

        return [label, ": ", .. displayedDefaultValue];
    }

    /// <summary>
    /// Gets an argument's default value to be displayed in help.
    /// </summary>
    /// <param name="symbol">The argument or option to get the default value for.</param>
    protected virtual IReadOnlyList<TextSegment> GetArgumentDefaultValue(Symbol symbol)
    {
        return symbol switch
        {
            Argument argument => ShouldShowDefaultValue(argument)
                ? ToSegments(argument.GetDefaultValue(), argument.ValueType)
                : [],
            Option option => ShouldShowDefaultValue(option)
                ? ToSegments(option.GetDefaultValue(), option.ValueType)
                : [],
            _ => ThrowHelper.ThrowArgumentException<IReadOnlyList<TextSegment>>(nameof(symbol), "Symbol must be an Argument or Option."),
        };

        static IReadOnlyList<TextSegment> ToSegments(object? value, Type valueType)
            => value switch
            {
                _ when (valueType == typeof(bool) || valueType == typeof(bool?)) && value is not true => [],
                bool boolValue => boolValue ? ["true"] : ["false"],
                null => [],
                string str => [str],
                IEnumerable enumerable => enumerable.Cast<object?>().Select(static v => v?.ToString()).NotNull().Select(static s => (TextSegment)s).Intersperse("|").ToList(),
                _ => [value.ToString() ?? string.Empty]
            };
    }

    /// <summary>
    /// Determines whether the default value for the specified symbol should be displayed in the help text.
    /// </summary>
    /// <param name="symbol">The symbol to check for a default value.</param>
    /// <returns>True if the default value should be shown; otherwise, false.</returns>
    protected virtual bool ShouldShowDefaultValue(Symbol symbol)
        => symbol switch
        {
            Option option => ShouldShowDefaultValue(option),
            Argument argument => ShouldShowDefaultValue(argument),
            _ => false
        };

    /// <summary>
    /// Determines whether the default value for the specified option should be displayed in the help text.
    /// </summary>
    /// <param name="option">The option to check for a default value.</param>
    /// <returns>True if the default value should be shown; otherwise, false.</returns>
    protected virtual bool ShouldShowDefaultValue(Option option)
        => option.HasDefaultValue;

    /// <summary>
    /// Determines whether the default value for the specified argument should be displayed in the help text.
    /// </summary>
    /// <param name="argument">The argument to check for a default value.</param>
    /// <returns>True if the default value should be shown; otherwise, false.</returns>
    protected virtual bool ShouldShowDefaultValue(Argument argument)
        => argument.HasDefaultValue;

    /// <summary>
    /// Specifies the target for which options should be retrieved when building the help text.
    /// </summary>
    [Flags]
    protected enum OptionsTarget
    {
        /// <summary>
        /// No options should be retrieved.
        /// </summary>
        None = 0,

        /// <summary>
        /// Non-recursive options defined on the current command should be retrieved.
        /// </summary>
        InstanceOptions = 1,

        /// <summary>
        /// Recursive options defined on the current command and its parent commands should be retrieved.
        /// </summary>
        RecursiveOptions = 2,

        /// <summary>
        /// All options, including both instance and recursive options, should be retrieved.
        /// </summary>
        All = InstanceOptions | RecursiveOptions,
    }
}
