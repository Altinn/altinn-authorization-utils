using Altinn.Authorization.CommandLine.Formatting.Json;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Tests.Formatting.Json;

public class JsonWithCommentsTextTests
{
    [Fact]
    public void Render_WhenJsonHasNoComments_UsesSingleLineWhenItFits()
    {
        var json = JsonWithCommentsText.Create("""{"a":1,"b":2}""");

        Render(json, maxWidth: 80).ShouldBe("""{"a": 1, "b": 2}""");
    }

    [Fact]
    public void Render_WhenObjectContainsComment_UsesMultipleLinesEvenWhenSingleLineFits()
    {
        var json = JsonWithCommentsText.Create("""
            {"a":1,// between
            "b":2}
            """);

        Render(json, maxWidth: 80).ShouldBe("""
            {
              "a": 1,
              // between
              "b": 2
            }
            """);
    }

    [Fact]
    public void Render_WhenArrayContainsComment_UsesMultipleLinesEvenWhenSingleLineFits()
    {
        var json = JsonWithCommentsText.Create("""
            [1,// between
            2]
            """);

        Render(json, maxWidth: 80).ShouldBe("""
            [
              1,
              // between
              2
            ]
            """);
    }

    [Fact]
    public void Render_WhenNestedValueContainsComment_ExpandsContainingPropertyAndObject()
    {
        var json = JsonWithCommentsText.Create("""
            {"a":[1,// between
            2]}
            """);

        Render(json, maxWidth: 80).ShouldBe("""
            {
              "a":
                [
                  1,
                  // between
                  2
                ]
            }
            """);
    }

    [Fact]
    public void Render_WhenContainerHasClosingComment_RendersCommentBeforeClose()
    {
        var json = JsonWithCommentsText.Create("""
            [1,// closing
            ]
            """);

        Render(json, maxWidth: 80).ShouldBe("""
            [
              1
              // closing
            ]
            """);
    }

    [Fact]
    public void Render_WhenCommentHasMultipleLines_RendersEachLineAsComment()
    {
        var json = JsonWithCommentsText.Create("""
            [/* first
            second */1]
            """);

        Render(json, maxWidth: 80).ShouldBe("""
            [
              // first
              // second
              1
            ]
            """);
    }

    [Fact]
    public void Render_WhenRepoCtlSampleJsonContainsComments_RendersCommentPositions()
    {
        var json = JsonWithCommentsText.Create(
            """
            // Pre comment
            {
                // before message
                "Message": "Hello, world!",
                "Timestamp": /* inline */ "2024-06-01T12:34:56Z",
                "Array": [1, 2, 3, 4, 5 /* past last item */], // after array
                "LongString": "This is a very long string that should be wrapped across multiple lines in the output to demonstrate the formatting capabilities of the JSON formatter. It contains enough characters to exceed typical line lengths and will be used to test how the formatter handles such cases.",
                "Nested": {
                    "Value": 42,
                    "Text": "Nested object"
                }
                // past last prop
            }
            // Post comment
            """);

        Render(json, maxWidth: 120).ShouldBe(string.Join(
            "\n",
            [
                "// Pre comment",
                "{",
                "  // before message",
                "  \"Message\": \"Hello, world!\",",
                "  // inline",
                "  \"Timestamp\": \"2024-06-01T12:34:56Z\",",
                "  \"Array\":",
                "    [",
                "      1,",
                "      2,",
                "      3,",
                "      4,",
                "      5",
                "      // past last item",
                "    ],",
                "  // after array",
                "  \"LongString\":",
                "    \"This is a very long string that should be wrapped across multiple lines in the output to demonstrate the formatting",
                "capabilities of the JSON formatter. It contains enough characters to exceed typical line lengths and will be used to ",
                "test how the formatter handles such cases.\",",
                "  \"Nested\": {",
                "    \"Value\": 42,",
                "    \"Text\": \"Nested object\"",
                "  }",
                "  // past last prop",
                "}",
                "// Post comment",
            ]));
    }

    private static string Render(JsonWithCommentsText json, int maxWidth)
    {
        var renderOptions = RenderOptions.Create(AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.NoColors,
        }));

        return string.Concat(((IRenderable)json)
            .Render(renderOptions, maxWidth)
            .Select(static segment => segment.Text));
    }
}
