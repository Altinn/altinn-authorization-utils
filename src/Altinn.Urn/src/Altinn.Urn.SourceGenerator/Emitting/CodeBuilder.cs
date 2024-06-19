using System.Runtime.CompilerServices;

namespace Altinn.Urn.SourceGenerator.Emitting;

internal readonly ref partial struct CodeBuilder
{
    private readonly static string SPACES = new(' ', 256);
    public static ReadOnlySpan<char> Indentation(int count) => SPACES.AsSpan(0, count);

    private readonly CodeStringBuilder _builder;
    private readonly ReadOnlySpan<char> _indentation;

    private CodeBuilder(CodeStringBuilder builder, int indentation)
    {
        _builder = builder;
        _indentation = Indentation(indentation);
    }

    public CodeBuilder(CodeStringBuilder builder)
    {
        _builder = builder;
        _indentation = ReadOnlySpan<char>.Empty;
    }

    public CodeBuilder Indent(int spaces = 4)
        => new(_builder, _indentation.Length + spaces);

    public void AppendLine()
    {
        _builder.AppendLine();
    }

    public bool IsAtStartOfLine => _builder.IsAtStartOfLine;

    private void Append(ReadOnlySpan<char> value)
    {
        if (_builder.IsAtStartOfLine)
        {
            _builder.Append(_indentation);
        }

        var newLineIndex = value.IndexOf('\n');
        if (newLineIndex == -1)
        {
            // fast path
            _builder.Append(value);
            return;
        }

        AppendLines(value, newLineIndex);
    }

    private void AppendLines(ReadOnlySpan<char> value, int newLineIndex)
    {
        while (newLineIndex != -1)
        {
            if (_builder.IsAtStartOfLine)
            {
                _builder.Append(_indentation);
            }

            _builder.Append(value.Slice(0, newLineIndex + 1));

            value = value.Slice(newLineIndex + 1);
            newLineIndex = value.IndexOf('\n');
        }

        if (!value.IsEmpty)
        {
            if (_builder.IsAtStartOfLine)
            {
                _builder.Append(_indentation);
            }

            _builder.Append(value);
        }
    }

    public void AppendLine(ReadOnlySpan<char> value)
    {
        Append(value);
        _builder.AppendLine();
    }

    public void AppendLine(string value)
    {
        Append(value.AsSpan());
        _builder.AppendLine();
    }

    /// <summary>Appends the specified interpolated string to this instance.</summary>
    /// <param name="handler">The interpolated string to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public void Append([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler handler) 
    {
        // Handled by the handler
    }

    /// <summary>Appends the specified interpolated string followed by the default line terminator to the end of the current StringBuilder object.</summary>
    /// <param name="handler">The interpolated string to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public void AppendLine([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler handler) 
    {
        _builder.AppendLine();
    }
}
