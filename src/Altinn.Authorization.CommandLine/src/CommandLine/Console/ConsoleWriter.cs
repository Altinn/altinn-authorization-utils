using System.Globalization;
using System.Text;
using CommunityToolkit.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Console;

internal sealed class ConsoleWriter
    : TextWriter
{
    private readonly IAnsiConsole _console;

    public ConsoleWriter(IAnsiConsole console)
        : base(CultureInfo.InvariantCulture)
    {
        _console = console;
    }

    public override Encoding Encoding
        => _console.Profile.Encoding;

    #region Write

    public sealed override void Write(char value)
        => Write([value]);

    public sealed override void Write(char[] buffer, int index, int count)
    {
        Guard.IsNotNull(buffer);
        Guard.IsGreaterThanOrEqualTo(index, 0);
        Guard.IsGreaterThanOrEqualTo(count, 0);
        if (buffer.Length - index < count)
        {
            ThrowHelper.ThrowArgumentException("The length of the buffer is less than the sum of index and count.");
        }

        Write(buffer.AsSpan(index, count));
    }

    public sealed override void Write(ReadOnlySpan<char> buffer)
        => Write(new string(buffer));

    public sealed override void Write(string? value)
    {
        if (value is null)
        {
            return;
        }

        _console.ExclusivityMode.Run(() =>
        {
            _console.Write(value);
            return 0;
        });
    }

    public void Write(IRenderable renderable)
    {
        _console.ExclusivityMode.Run(() =>
        {
            _console.Write(renderable);
            return 0;
        });
    }

    public sealed override void Write(object? value)
    {
        switch (value)
        {
            case null: return;
            case string s: Write(s); return;
            case IRenderable r: Write(r); return;
            case IFormattable f: Write(f.ToString(null, FormatProvider)); return;
            default: Write(value.ToString()); return;
        }
    }

    public sealed override void Write(StringBuilder? value)
    {
        if (value is null)
        {
            return;
        }

        Write(value.ToString());
    }

    #endregion

    #region WriteLine

    public sealed override void WriteLine(char value)
        => WriteLine([value]);

    public sealed override void WriteLine(char[] buffer, int index, int count)
    {
        Guard.IsNotNull(buffer);
        Guard.IsGreaterThanOrEqualTo(index, 0);
        Guard.IsGreaterThanOrEqualTo(count, 0);
        if (buffer.Length - index < count)
        {
            ThrowHelper.ThrowArgumentException("The length of the buffer is less than the sum of index and count.");
        }

        WriteLine(buffer.AsSpan(index, count));
    }

    public sealed override void WriteLine(ReadOnlySpan<char> buffer)
    {
        var value = string.Create(
            length: buffer.Length + NewLine.Length,
            new WriteLineContext(buffer, NewLine),
            (span, ctx) =>
            {
                ctx.Buffer.CopyTo(span);
                ctx.NewLine.CopyTo(span.Slice(ctx.Buffer.Length));
            });

        Write(value);
    }

    public sealed override void WriteLine(string? value)
    {
        if (value is null)
        {
            return;
        }

        WriteLine(value.AsSpan());
    }

    public void WriteLine(IRenderable renderable)
    {
        _console.ExclusivityMode.Run(() =>
        {
            _console.Write(renderable);
            _console.WriteLine();
            return 0;
        });
    }

    public sealed override void WriteLine(object? value)
    {
        switch (value)
        {
            case null: return;
            case string s: WriteLine(s); return;
            case IRenderable r: WriteLine(r); return;
            case IFormattable f: WriteLine(f.ToString(null, FormatProvider)); return;
            default: WriteLine(value.ToString()); return;
        }
    }

    public sealed override void WriteLine(StringBuilder? value)
    {
        if (value is null)
        {
            return;
        }

        var newLine = NewLine;
        value.Append(newLine);
        var str = value.ToString();
        value.Length -= newLine.Length;

        Write(str);
    }

    #endregion

    private readonly ref struct WriteLineContext(ReadOnlySpan<char> buffer, ReadOnlySpan<char> newLine)
    {
        public readonly ReadOnlySpan<char> Buffer { get; } = buffer;
        public readonly ReadOnlySpan<char> NewLine { get; } = newLine;
    }
}
