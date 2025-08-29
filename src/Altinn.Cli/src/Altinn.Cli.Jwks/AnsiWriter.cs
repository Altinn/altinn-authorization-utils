using CommunityToolkit.Diagnostics;
using Spectre.Console;
using System.Text;

namespace Altinn.Cli.Jwks;

internal sealed class AnsiWriter(IAnsiConsole console)
    : TextWriter
{
    public override Encoding Encoding => console.Profile.Encoding;

    public override void Write(char value)
    {
        ThrowHelper.ThrowNotSupportedException("Don't write individual characters, use strings instead.");
    }

    public override void Write(string? value)
    {
        console.Write(value ?? string.Empty);
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        Write(new string(buffer));
    }

    public override void Write(char[] buffer, int index, int count)
    {
        Write(buffer.AsSpan(index, count));
    }
}
