using System.Text;

namespace Altinn.Urn.SourceGenerator.Emitting;

internal sealed class CodeStringBuilder
    : IDisposable
{
    public static CodeStringBuilder Rent()
    {
        var instance = _cachedBuilder;
        _cachedBuilder = null;

        if (instance is null)
        {
            instance = new(new StringBuilder());
        }

        return instance;
    }

    [ThreadStatic]
    private static CodeStringBuilder? _cachedBuilder;

    public StringBuilder StringBuilder { get; }

    public bool IsAtStartOfLine { get; private set; } = true;

    private CodeStringBuilder(StringBuilder builder)
    {
        StringBuilder = builder;
    }

    void IDisposable.Dispose()
    {
        StringBuilder.Clear();
        IsAtStartOfLine = true;
        _cachedBuilder = this;
    }

    public override string ToString()
        => StringBuilder.ToString();

    public CodeStringBuilder AppendLine()
    {
        StringBuilder.Append('\n');

        IsAtStartOfLine = true;
        return this;
    }

    public CodeStringBuilder Append(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return this;
        }

        unsafe
        {
            fixed (char* ptr = value)
            {
                StringBuilder.Append(ptr, value.Length);
            }
        }

        IsAtStartOfLine = value[^1] == '\n';
        return this;
    }
}
