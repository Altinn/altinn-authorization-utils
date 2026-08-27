using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Utils;
using CommunityToolkit.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Formatting.Pretty;

// union Notation {
//     Newline,
//     Text(Segment, u32),
//     Flat(Notation),
//     MustBreak(Notation),
//     Indent(u32, Notation),
//     Concat(Notation, Notation),
//     Choice(Notation, Notation),
// }
/// <summary>
/// Represents a notation for pretty-printing text with optional styling and formatting.
/// </summary>
public sealed class Notation
    : JustInTimeSizeDependentRenderable
{
    private static readonly SearchValues<char> NewLineCharacters
        = SearchValues.Create(['\r', '\n']);

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents a newline.
    /// </summary>
    public static Notation Newline { get; } = new(Kind.Newline);

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents a forced newline.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Newline"/>, a hard line prevents the containing notation from fitting on a single line.
    /// </remarks>
    public static Notation HardLine { get; } = new(Kind.MustBreak, left: Newline);

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents an empty text segment.
    /// </summary>
    public static Notation Empty { get; } = Text(string.Empty);

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents a text segment with the specified text and style.
    /// </summary>
    /// <param name="text">The text of the segment.</param>
    /// <remarks>
    /// The <paramref name="text"/> should not contain any newline characters.
    /// </remarks>
    /// <returns>A <see cref="Notation"/> representing the text segment.</returns>
    public static Notation Text(string text)
        => Text(text, Style.Plain);

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents a text segment with the specified text and style.
    /// </summary>
    /// <param name="text">The text of the segment.</param>
    /// <param name="style">The style of the segment.</param>
    /// <remarks>
    /// The <paramref name="text"/> should not contain any newline characters.
    /// </remarks>
    /// <returns>A <see cref="Notation"/> representing the text segment.</returns>
    public static Notation Text(string text, Style style)
    {
        if (text.AsSpan().ContainsAny(NewLineCharacters))
        {
            ThrowHelper.ThrowArgumentException(nameof(text), "Text cannot contain newline characters.");
        }

        var segment = new Segment(text, style);
        var cellCount = segment.CellCount();
        return new(Kind.Text, segment, checked((uint)cellCount));
    }

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents a flat notation with the specified inner notation.
    /// </summary>
    /// <param name="inner">The inner notation.</param>
    /// <returns>A <see cref="Notation"/> representing the flat notation.</returns>
    public static Notation Flat(Notation inner)
        => new(Kind.Flat, left: inner);

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents an indented notation with the specified size and inner notation.
    /// </summary>
    /// <param name="size">The indentation size.</param>
    /// <param name="inner">The inner notation.</param>
    /// <returns>A <see cref="Notation"/> representing the indented notation.</returns>
    public static Notation Indent(uint size, Notation inner)
        => new(Kind.Indent, num: size, left: inner);

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents a concatenation of two notations.
    /// </summary>
    /// <param name="left">The left notation.</param>
    /// <param name="right">The right notation.</param>
    /// <returns>A <see cref="Notation"/> representing the concatenation of the two notations.</returns>
    public static Notation Concat(Notation left, Notation right)
        => new(Kind.Concat, left: left, right: right);

    /// <summary>
    /// Gets a <see cref="Notation"/> that represents a choice between two notations.
    /// </summary>
    /// <param name="left">The left notation.</param>
    /// <param name="right">The right notation.</param>
    /// <remarks>
    /// In every choise (<c>x | y</c>), the shortest possible first line of <c>y</c> must be at
    /// least as short as <strong>every</strong> possible first line of <c>x</c>.
    /// </remarks>
    /// <returns>A <see cref="Notation"/> representing the choice between the two notations.</returns>
    public static Notation Choice(Notation left, Notation right)
        => new(Kind.Choice, left: left, right: right);

    private readonly Kind _kind;
    private readonly Segment? _text;
    private readonly uint _num; // text length or indent size
    private readonly Notation? _left; // Flat(1) or Indent(2) or Concat(1) or Choice(1)
    private readonly Notation? _right; // Concat(2) or Choice(2)
    private readonly bool _mustBreak;

    private Notation(
        Kind kind,
        Segment? text = null,
        uint num = 0,
        Notation? left = null,
        Notation? right = null)
    {
        _kind = kind;
        _text = text;
        _num = num;
        _left = left;
        _right = right;
        _mustBreak = kind switch
        {
            Kind.MustBreak => true,
            Kind.Flat or Kind.Indent => left!._mustBreak,
            Kind.Concat => left!._mustBreak || right!._mustBreak,
            Kind.Choice => left!._mustBreak && right!._mustBreak,
            _ => false,
        };
    }

    internal bool MustBreak
        => _mustBreak;

    internal bool IsNewline()
        => _kind is Kind.Newline;

    internal bool IsText([NotNullWhen(true)] out Segment? segment, out uint length)
    {
        if (_kind == Kind.Text)
        {
            segment = _text!;
            length = _num;
            return true;
        }

        segment = null;
        length = 0;
        return false;
    }

    internal bool IsFlat([NotNullWhen(true)] out Notation? inner)
    {
        if (_kind == Kind.Flat)
        {
            inner = _left!;
            return true;
        }

        inner = null;
        return false;
    }

    internal bool IsMustBreak([NotNullWhen(true)] out Notation? inner)
    {
        if (_kind == Kind.MustBreak)
        {
            inner = _left!;
            return true;
        }

        inner = null;
        return false;
    }

    internal bool IsIndent(out uint size, [NotNullWhen(true)] out Notation? inner)
    {
        if (_kind == Kind.Indent)
        {
            size = _num;
            inner = _left!;
            return true;
        }

        size = 0;
        inner = null;
        return false;
    }

    internal bool IsConcat([NotNullWhen(true)] out Notation? left, [NotNullWhen(true)] out Notation? right)
    {
        if (_kind == Kind.Concat)
        {
            left = _left!;
            right = _right!;
            return true;
        }

        left = null;
        right = null;
        return false;
    }

    internal bool IsChoice([NotNullWhen(true)] out Notation? left, [NotNullWhen(true)] out Notation? right)
    {
        if (_kind == Kind.Choice)
        {
            left = _left!;
            right = _right!;
            return true;
        }

        left = null;
        right = null;
        return false;
    }

    /// <inheritdoc/>
    protected override IRenderable Build(int maxWidth)
        => PrettyPrinter.Print(this, maxWidth);

    /// <inheritdoc cref="Concat(Notation, Notation)"/>
    public static Notation operator +(Notation left, Notation right)
        => Concat(left, right);

    /// <inheritdoc cref="Choice(Notation, Notation)"/>
    public static Notation operator |(Notation left, Notation right)
        => Choice(left, right);

    internal enum Kind
    {
        Newline,
        Text,
        Flat,
        MustBreak,
        Indent,
        Concat,
        Choice,
    }
}
