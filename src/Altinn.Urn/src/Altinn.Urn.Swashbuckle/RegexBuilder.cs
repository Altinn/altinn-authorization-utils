using System.Buffers;
using System.Text;

namespace Altinn.Urn.Swashbuckle;

internal class RegexBuilder
{
    private static readonly ThreadLocal<RegexBuilder> _threadStaticInstance
        = new(() => new RegexBuilder(256));

    public static RegexBuilder ThreadStaticInstance 
        => _threadStaticInstance.Value!;

    private static readonly SearchValues<char> _metachars =
            SearchValues.Create("\t\n\f\r #$()*+.?[\\^{|");

    private static int IndexOfMetachar(ReadOnlySpan<char> input) 
        => input.IndexOfAny(_metachars);

    private readonly StringBuilder _builder;

    private RegexBuilder(int capacity)
    {
        _builder = new(capacity);
    }

    public override string ToString()
        => _builder.ToString();

    public void Clear()
    {
        _builder.Clear();
    }

    public void AppendRaw(string value)
    {
        _builder.Append(value);
    }

    public void AppendRaw(char value)
    {
        _builder.Append(value);
    }

    public void AppendEscaped(ReadOnlySpan<char> value)
    {
        int indexOfMetachar = IndexOfMetachar(value);
        if (indexOfMetachar == -1)
        {
            _builder.Append(value);
            return;
        }

        AppendEscapedImpl(value, indexOfMetachar);

        void AppendEscapedImpl(ReadOnlySpan<char> value, int indexOfMetachar)
        {
            while (true)
            {
                _builder.Append(value.Slice(0, indexOfMetachar));
                value = value.Slice(indexOfMetachar);

                if (value.IsEmpty)
                {
                    break;
                }

                char ch = value[0];

                switch (ch)
                {
                    case '\n':
                        ch = 'n';
                        break;
                    case '\r':
                        ch = 'r';
                        break;
                    case '\t':
                        ch = 't';
                        break;
                    case '\f':
                        ch = 'f';
                        break;
                }

                _builder.Append('\\');
                _builder.Append(ch);
                value = value.Slice(1);

                indexOfMetachar = IndexOfMetachar(value);
                if (indexOfMetachar < 0)
                {
                    indexOfMetachar = value.Length;
                }
            }
        }
    }
}
