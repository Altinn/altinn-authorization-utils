using System.Collections.Immutable;
using Altinn.Authorization.CommandLine.Formatting.Pretty;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal abstract record JsonContainerNode
    : JsonWithCommentsNode
{
    private static readonly Notation Space = Notation.Text(" ");
    private static readonly Notation Comma = Notation.Text(",", JsonStyles.Comma);

    protected ImmutableArray<string> ClosingCommentLines { get; private init; } = [];

    private protected bool IsEmpty { get; }

    private protected abstract Notation Open { get; }
    private protected abstract Notation Close { get; }
    private protected abstract IReadOnlyList<Notation> GetChildren(in JsonRenderOptions renderOptions);

    internal JsonContainerNode WithClosingCommentLines(ImmutableArray<string> commentLines)
    {
        if (ClosingCommentLines.IsEmpty && commentLines.IsDefaultOrEmpty)
        {
            return this;
        }

        return this with { ClosingCommentLines = [.. ClosingCommentLines, .. commentLines] };
    }

    private protected override sealed Notation ToNotationInner(in JsonRenderOptions renderOptions)
    {
        var children = GetChildren(in renderOptions);
        if (children.Count == 0)
        {
            if (!ClosingCommentLines.IsEmpty)
            {
                return Open
                    + Notation.Indent(renderOptions.IndentSize, Notation.HardLine + CommentLines(ClosingCommentLines))
                    + Notation.HardLine
                    + Close;
            }

            return Open + Space + Close;
        }

        var singleLine = Open + CommaSepSingleLine(children) + Close;
        var contents = CommaSepMultiLine(children);
        if (!ClosingCommentLines.IsEmpty)
        {
            contents += Notation.HardLine + CommentLines(ClosingCommentLines);
        }

        var multiLine = Open + Notation.Indent(renderOptions.IndentSize, Notation.Newline + contents) + Notation.Newline + Close;
        if (!ClosingCommentLines.IsEmpty)
        {
            return multiLine;
        }

        return singleLine | multiLine;
    }

    private static Notation CommaSepSingleLine(IReadOnlyList<Notation> children)
    {
        var enumerator = children.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            ThrowHelper.ThrowArgumentException(nameof(children), "Children collection must not be empty.");
        }

        var list = Notation.Flat(enumerator.Current);
        while (enumerator.MoveNext())
        {
            list = list + Comma + Space + Notation.Flat(enumerator.Current);
        }

        return list;
    }

    private static Notation CommaSepMultiLine(IReadOnlyList<Notation> children)
    {
        var enumerator = children.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            ThrowHelper.ThrowArgumentException(nameof(children), "Children collection must not be empty.");
        }

        var list = enumerator.Current;
        while (enumerator.MoveNext())
        {
            list = list + Comma + Notation.Newline + enumerator.Current;
        }

        return list;
    }
}
