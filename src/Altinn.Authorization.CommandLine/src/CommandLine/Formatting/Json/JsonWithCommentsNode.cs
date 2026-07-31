using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Pretty;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal abstract record JsonWithCommentsNode
{
    protected ImmutableArray<string> LeadingCommentLines { get; private init; } = [];

    protected ImmutableArray<string> TrailingCommentLines { get; private init; } = [];

    internal protected abstract void Write(Utf8JsonWriter writer);

    private protected abstract Notation ToNotationInner(in JsonRenderOptions renderOptions);

    internal JsonWithCommentsNode WithLeadingCommentLines(ImmutableArray<string> commentLines)
    {
        Debug.Assert(LeadingCommentLines.IsEmpty);

        if (commentLines.IsDefaultOrEmpty)
        {
            return this;
        }

        return this with { LeadingCommentLines = commentLines };
    }

    internal JsonWithCommentsNode WithTrailingCommentLines(ImmutableArray<string> commentLines)
    {
        if (TrailingCommentLines.IsEmpty && commentLines.IsDefaultOrEmpty)
        {
            return this;
        }

        return this with { TrailingCommentLines = [.. TrailingCommentLines, .. commentLines] };
    }

    internal Notation ToNotation(JavaScriptEncoder encoder, uint indentSize)
    {
        var renderOptions = new JsonRenderOptions
        {
            Encoder = encoder,
            IndentSize = indentSize,
        };

        return ToNotation(in renderOptions);
    }

    internal Notation ToNotation(in JsonRenderOptions renderOptions)
    {
        var inner = ToNotationInner(in renderOptions);

        if (LeadingCommentLines.IsEmpty && TrailingCommentLines.IsEmpty)
        {
            return inner;
        }

        if (!LeadingCommentLines.IsEmpty)
        {
            inner = CommentLines(LeadingCommentLines) + Notation.HardLine + inner;
        }

        if (!TrailingCommentLines.IsEmpty)
        {
            inner += Notation.HardLine + CommentLines(TrailingCommentLines);
        }

        return inner;
    }

    private protected static Notation CommentLines(ImmutableArray<string> commentLines)
    {
        Debug.Assert(!commentLines.IsDefaultOrEmpty);

        var comments = CommentLine(commentLines[0]);
        for (var i = 1; i < commentLines.Length; i++)
        {
            comments += Notation.HardLine + CommentLine(commentLines[i]);
        }

        return comments;
    }

    private static Notation CommentLine(string line)
        => Notation.Text($"// {line.AsSpan().Trim()}", JsonStyles.Comment);

    internal protected readonly record struct JsonRenderOptions
    {
        public required uint IndentSize { get; init; }
        public required JavaScriptEncoder Encoder { get; init; }
    }
}
