using System.CommandLine;
using System.CommandLine.Parsing;
using Altinn.Authorization.CommandLine.Help;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Tests.Help;

public class HelpUsageTests
{
    [Fact]
    public void CustomDisplay_UsesContextArgumentColor()
    {
        var customization = HelpDisplayArgumentCustomization.Create(["vertical"]);

        foreach (var color in new[] { Color.Cyan, Color.Magenta })
        {
            var segments = customization.CustomDisplay!(new(color)).ToArray();
            segments.Single(segment => segment.Text == "vertical").Style!.Value.Foreground.ShouldBe(color);
        }
    }

    [Theory]
    [InlineData(false, "[<selector>...]")]
    [InlineData(true, "[<selector>]")]
    public void Usage_UsesDisplayArityWithoutChangingParserArity(bool overrideArity, string expected)
    {
        var command = new Command("test");
        var argument = new Argument<string?>("vertical") { Arity = ArgumentArity.ZeroOrMore };
        argument.HelpCustomization.Argument = HelpDisplayArgumentCustomization.Create(
            ["selector"], arity: overrideArity ? ArgumentArity.ZeroOrOne : null);
        command.Arguments.Add(argument);

        using var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(writer),
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Enrichment = new() { UseDefaultEnrichers = false },
        });
        console.Write(new UsageHelpBuilder().Usage(command.Parse("").CommandResult));

        writer.ToString().ShouldContain(expected);
        argument.Arity.ShouldBe(ArgumentArity.ZeroOrMore);
    }

    private sealed class UsageHelpBuilder : DefaultHelpBuilder
    {
        public IRenderable Usage(CommandResult result) => GetUsage(result)!;
    }
}
