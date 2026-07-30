using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Formatting.Pretty;

/// <summary>
/// Formats a <see cref="Notation"/> into a <see cref="Paragraph"/>.
/// The implementation is based on a Wadler-Lindig pretty-printing algorithm,
/// with modifications from https://justinpombrio.net/2024/02/23/a-twist-on-Wadlers-printer.html,
/// and a MustBreak marker to poison line-fitting decisions.
/// </summary>
internal static class PrettyPrinter
{
    public static Paragraph Print(Notation notation, int maxWidth)
    {
        // Maximum line width that we'll try to stay within
        uint width = checked((uint)maxWidth);

        // Current column position
        uint col = 0;

        // A stack of chunks to print. The _top_ of the stack is the
        // _end_ of the vector, which represents the _earliest_ part
        // of the document to print.
        List<Chunk> chunks = [new Chunk(notation, 0, default)];

        // Output paragraph that will contain the rendered segments
        Paragraph output = new();

        Segment? segment;
        uint size;
        Notation? left;
        Notation? right;
        while (chunks.TryPop(out var chunk))
        {
            if (chunk.Notation.IsText(out segment, out size))
            {
                output.Append(segment.Text, segment.Style, segment.Link);
                col += size;
            }
            else if (chunk.Notation.IsNewline())
            {
                output.Append("\n");
                output.Append(new string(' ', (int)chunk.Indent));
                col = chunk.Indent;
            }
            else if (chunk.Notation.IsFlat(out left))
            {
                chunks.Push(chunk.WithNotation(left).AsFlat());
            }
            else if (chunk.Notation.IsMustBreak(out left))
            {
                chunks.Push(chunk.WithNotation(left));
            }
            else if (chunk.Notation.IsIndent(out size, out left))
            {
                chunks.Push(chunk.WithNotation(left).Indented(size));
            }
            else if (chunk.Notation.IsConcat(out left, out right))
            {
                chunks.Push(chunk.WithNotation(right));
                chunks.Push(chunk.WithNotation(left));
            }
            else if (chunk.Notation.IsChoice(out left, out right))
            {
                var leftChunk = chunk.WithNotation(left);
                var rightChunk = chunk.WithNotation(right);
                if (chunk.Flat || Fits(col, width, leftChunk, chunks.AsSpan()))
                {
                    chunks.Push(leftChunk);
                }
                else
                {
                    chunks.Push(rightChunk);
                }
            }
            else
            {
                Unreachable();
            }
        }

        return output;
    }

    private static bool Fits(uint col, uint width, Chunk chunk, ReadOnlySpan<Chunk> chunks)
    {
        if (col > width)
        {
            return false;
        }

        var remaining = width - col;
        Stack<Chunk> stack = new();
        stack.Push(chunk);

        uint size;
        Notation? left;
        Notation? right;
        while (true)
        {
            if (!stack.TryPop(out chunk))
            {
                if (!chunks.TryPop(out chunk))
                {
                    return true;
                }
            }

            if (chunk.Notation.MustBreak)
            {
                return false;
            }

            if (chunk.Notation.IsMustBreak(out _))
            {
                return false;
            }
            else if (chunk.Notation.IsNewline())
            {
                return true;
            }
            else if (chunk.Notation.IsText(out _, out size))
            {
                if (size > remaining)
                {
                    return false;
                }

                remaining -= size;
            }
            else if (chunk.Notation.IsFlat(out left))
            {
                stack.Push(chunk.WithNotation(left).AsFlat());
            }
            else if (chunk.Notation.IsIndent(out size, out left))
            {
                stack.Push(chunk.WithNotation(left).Indented(size));
            }
            else if (chunk.Notation.IsConcat(out left, out right))
            {
                stack.Push(chunk.WithNotation(right));
                stack.Push(chunk.WithNotation(left));
            }
            else if (chunk.Notation.IsChoice(out left, out right))
            {
                if (chunk.Flat)
                {
                    stack.Push(chunk.WithNotation(left));
                }
                else
                {
                    // Relies on the rule that for every choice (`x | y`),
                    // the first line of `y` is no longer than the first line of `x`.
                    stack.Push(chunk.WithNotation(right));
                }
            }
            else
            {
                Unreachable();
            }
        }
    }

    [DoesNotReturn]
    private static void Unreachable()
    {
        throw new UnreachableException();
    }

    extension(List<Chunk> chunks)
    {
        private void Push(Chunk chunk)
        {
            chunks.Add(chunk);
        }

        private bool TryPop(out Chunk chunk)
        {
            if (chunks.Count > 0)
            {
                chunk = chunks[^1];
                chunks.RemoveAt(chunks.Count - 1);
                return true;
            }

            chunk = default;
            return false;
        }

        private ReadOnlySpan<Chunk> AsSpan()
        {
            return CollectionsMarshal.AsSpan(chunks);
        }
    }

    extension(ref ReadOnlySpan<Chunk> chunks)
    {
        private bool TryPop(out Chunk chunk)
        {
            if (chunks.Length > 0)
            {
                chunk = chunks[^1];
                chunks = chunks[..^1];
                return true;
            }

            chunk = default;
            return false;
        }
    }

    private readonly record struct Chunk(Notation Notation, uint Indent, Flags Flags)
    {
        public bool Flat
            => (Flags & Flags.Flat) != 0;

        public Chunk WithNotation(Notation notation)
            => new(notation, Indent, Flags);

        public Chunk Indented(uint indent)
            => new(Notation, Indent + indent, Flags);

        public Chunk AsFlat()
            => new(Notation, Indent, Flags | Flags.Flat);
    }

    [Flags]
    private enum Flags
        : byte
    {
        None = 0,
        Flat = 1 << 0,
    }
}
