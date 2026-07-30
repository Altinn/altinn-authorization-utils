using Altinn.Authorization.CommandLine.Formatting.Pretty;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Tests.Formatting.Pretty;

public class PrettyPrinterTests
{
    [Fact]
    public void Print_WhenChoiceFits_UsesFirstAlternative()
    {
        var notation = Notation.Text("abcd") | Notation.Text("x");

        Render(notation, maxWidth: 4).ShouldBe("abcd");
    }

    [Fact]
    public void Print_WhenChoiceDoesNotFit_UsesSecondAlternative()
    {
        var notation = Notation.Text("abcd") | Notation.Text("x");

        Render(notation, maxWidth: 3).ShouldBe("x");
    }

    [Fact]
    public void Print_WhenQueuedSuffixWouldOverflow_UsesSecondAlternative()
    {
        var notation = (Notation.Text("abc") | Notation.Text("a")) + Notation.Text("def");

        Render(notation, maxWidth: 5).ShouldBe("adef");
    }

    [Fact]
    public void Print_WhenChoiceContainsNewline_OnlyChecksFirstLine()
    {
        var notation = (Notation.Text("abc") + Notation.Newline + Notation.Text("def")) | Notation.Text("x");

        Render(notation, maxWidth: 3).ShouldBe("""
            abc
            def
            """);
    }

    [Fact]
    public void Print_WhenChoiceContainsHardLine_UsesSecondAlternative()
    {
        var notation = (Notation.Text("abc") + Notation.HardLine + Notation.Text("def")) | Notation.Text("x");

        Render(notation, maxWidth: 100).ShouldBe("x");
    }

    [Fact]
    public void Print_WhenFlatContainsChoice_UsesFirstAlternative()
    {
        var notation = Notation.Flat(Notation.Text("abcd") | Notation.Text("x"));

        Render(notation, maxWidth: 1).ShouldBe("abcd");
    }

    [Fact]
    public void Print_WhenIndentedAfterNewline_InsertsIndentation()
    {
        var notation = Notation.Text("[")
            + Notation.Indent(2, Notation.Newline + Notation.Text("x"))
            + Notation.Newline
            + Notation.Text("]");

        Render(notation, maxWidth: 100).ShouldBe("""
            [
              x
            ]
            """);
    }

    [Fact]
    public void Text_WhenTextContainsNewline_Throws()
    {
        Should.Throw<ArgumentException>(() => Notation.Text("a\nb"));
    }

    private static string Render(Notation notation, int maxWidth)
    {
        var renderOptions = RenderOptions.Create(AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
        }));

        return string.Concat(((IRenderable)PrettyPrinter.Print(notation, maxWidth))
            .Render(renderOptions, 1000)
            .Select(static segment => segment.Text));
    }
}
