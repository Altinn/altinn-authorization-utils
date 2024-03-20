//HintName: MyUrn.g.cs
#nullable enable

using Altinn.Urn;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MyNamespace;

[DebuggerDisplay("{DebuggerDisplay}")]
[System.Text.Json.Serialization.JsonConverterAttribute(typeof(Altinn.Urn.UrnJsonConverter<MyNamespace.MyUrn>))]
partial record MyUrn
    : IParsable<MyUrn>
    , ISpanParsable<MyUrn>
    , IFormattable
    , ISpanFormattable
    , IUrn<MyUrn, MyUrn.Type>
{
    private static readonly ImmutableArray<Type> _variants = [
        Type.Test1,
        Type.Test2,
        Type.Test2Sub,
        Type.Test3,
        Type.Test4,
    ];

    private static readonly ImmutableArray<string> _validPrefixes = [
        "urn:altinn:test1",
        "urn:altinn:test2",
        "urn:altinn:test2:sub",
        "urn:altinn:test2:sub1",
        "urn:altinn:test2:sub2",
        "urn:notaltinn",
        "urn:nroot",
    ];

    /// <inheritdoc/>
    [CompilerGenerated]
    public static ReadOnlySpan<Type> Variants => _variants.AsSpan();

    /// <inheritdoc/>
    [CompilerGenerated]
    public static ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

    /// <inheritdoc/>
    [CompilerGenerated]
    public static ReadOnlySpan<string> PrefixesFor(Type type)
        => type switch {
            Type.Test1 => Test1.Prefixes,
            Type.Test2 => Test2.Prefixes,
            Type.Test2Sub => Test2Sub.Prefixes,
            Type.Test3 => Test3.Prefixes,
            Type.Test4 => Test4.Prefixes,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type ValueTypeFor(Type type)
        => type switch {
            Type.Test1 => typeof(System.Guid),
            Type.Test2 => typeof(int),
            Type.Test2Sub => typeof(float),
            Type.Test3 => typeof(long),
            Type.Test4 => typeof(uint),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type VariantTypeFor(Type type)
        => type switch {
            Type.Test1 => typeof(Test1),
            Type.Test2 => typeof(Test2),
            Type.Test2Sub => typeof(Test2Sub),
            Type.Test3 => typeof(Test3),
            Type.Test4 => typeof(Test4),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string NameFor(Type type)
        => type switch {
            Type.Test1 => nameof(Test1),
            Type.Test2 => nameof(Test2),
            Type.Test2Sub => nameof(Test2Sub),
            Type.Test3 => nameof(Test3),
            Type.Test4 => nameof(Test4),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    private readonly string _urn;
    private readonly int _valueIndex;
    private readonly Type _type;

    [CompilerGenerated]
    private MyUrn(string urn, int valueIndex, Type type) => (_urn, _valueIndex, _type) = (urn, valueIndex, type);

    /// <inheritdoc/>
    [CompilerGenerated]
    public string Urn => _urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public Type UrnType => _type;

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlySpan<char> AsSpan() => _urn.AsSpan();

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlyMemory<char> AsMemory() => _urn.AsMemory();

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlySpan<char> ValueSpan => _urn.AsSpan(_valueIndex);

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlyMemory<char> ValueMemory => _urn.AsMemory(_valueIndex);

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlySpan<char> PrefixSpan => _urn.AsSpan(0, _valueIndex - 1);

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlyMemory<char> PrefixMemory => _urn.AsMemory(0, _valueIndex - 1);

    /// <inheritdoc/>
    [CompilerGenerated]
    public string ToString(string? format, IFormatProvider? provider)
    {
        switch (format.AsSpan())
        {
            case ['P']:
                return new string(PrefixSpan);

            case ['S']:
                return new string(ValueSpan);

            case ['V', ..var valueFormatSpan]:
                var valueFormat = valueFormatSpan.Length == 0 ? null : new string(valueFormatSpan);
                return _type switch
                {
                    Type.Test1 => FormatTest1(((Test1)this).Value, valueFormat, provider),
                    Type.Test2 => FormatTest2(((Test2)this).Value, valueFormat, provider),
                    Type.Test2Sub => FormatTest2Sub(((Test2Sub)this).Value, valueFormat, provider),
                    Type.Test3 => FormatTest3(((Test3)this).Value, valueFormat, provider),
                    Type.Test4 => FormatTest4(((Test4)this).Value, valueFormat, provider),
                    _ => Unreachable<string>(),
                };

            case []:
            case ['R']:
            case ['G']:
            default:
                return _urn;
        }
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        switch (format)
        {
            case ['P']:
                return TryAppend(destination, out charsWritten, PrefixSpan);

            case ['S']:
                return TryAppend(destination, out charsWritten, ValueSpan);

            case ['V', ..var valueFormatSpan]:
                charsWritten = 0;
                return _type switch
                {
                    Type.Test1 => TryFormatTest1(((Test1)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    Type.Test2 => TryFormatTest2(((Test2)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    Type.Test2Sub => TryFormatTest2Sub(((Test2Sub)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    Type.Test3 => TryFormatTest3(((Test3)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    Type.Test4 => TryFormatTest4(((Test4)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    _ => Unreachable<bool>(),
                };

            case []:
            case ['R']:
            case ['G']:
            default:
                return TryAppend(destination, out charsWritten, _urn.AsSpan());
        }
    }

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsTest1([MaybeNullWhen(false)] out System.Guid value)
    #pragma warning restore CS8826
    {
        if (_type == Type.Test1)
        {
            value = ((Test1)this).Value;
            return true;
        }

        value = default;
        return false;
    }

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsTest2([MaybeNullWhen(false)] out int value)
    #pragma warning restore CS8826
    {
        if (_type == Type.Test2)
        {
            value = ((Test2)this).Value;
            return true;
        }

        value = default;
        return false;
    }

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsTest2Sub([MaybeNullWhen(false)] out float value)
    #pragma warning restore CS8826
    {
        if (_type == Type.Test2Sub)
        {
            value = ((Test2Sub)this).Value;
            return true;
        }

        value = default;
        return false;
    }

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsTest3([MaybeNullWhen(false)] out long value)
    #pragma warning restore CS8826
    {
        if (_type == Type.Test3)
        {
            value = ((Test3)this).Value;
            return true;
        }

        value = default;
        return false;
    }

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsTest4([MaybeNullWhen(false)] out uint value)
    #pragma warning restore CS8826
    {
        if (_type == Type.Test4)
        {
            value = ((Test4)this).Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public override string ToString() => _urn;

    [CompilerGenerated]
    protected string DebuggerDisplay => _urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public virtual bool Equals(MyUrn? other) => other is not null && _type == other._type && _urn == other._urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public override int GetHashCode() => _urn.GetHashCode();

    [CompilerGenerated]
    private static bool TryParseTest1(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out System.Guid value)
        => System.Guid.TryParse(segment, provider, out value);

    [CompilerGenerated]
    private static bool TryParseTest2(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out int value)
        => int.TryParse(segment, provider, out value);

    [CompilerGenerated]
    private static bool TryParseTest2Sub(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out float value)
        => float.TryParse(segment, provider, out value);

    [CompilerGenerated]
    private static bool TryParseTest3(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out long value)
        => long.TryParse(segment, provider, out value);

    [CompilerGenerated]
    private static bool TryParseTest4(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out uint value)
        => uint.TryParse(segment, provider, out value);

    [CompilerGenerated]
    private static string FormatTest1(System.Guid value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    [CompilerGenerated]
    private static bool TryFormatTest1(System.Guid value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);

    [CompilerGenerated]
    private static string FormatTest2(int value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    [CompilerGenerated]
    private static bool TryFormatTest2(int value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);

    [CompilerGenerated]
    private static string FormatTest2Sub(float value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    [CompilerGenerated]
    private static bool TryFormatTest2Sub(float value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);

    [CompilerGenerated]
    private static string FormatTest3(long value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    [CompilerGenerated]
    private static bool TryFormatTest3(long value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);

    [CompilerGenerated]
    private static string FormatTest4(uint value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    [CompilerGenerated]
    private static bool TryFormatTest4(uint value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);

    [CompilerGenerated]
    private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, string? original, [MaybeNullWhen(false)] out MyUrn result)
    {
        if (s.StartsWith("urn:"))
        {
            var s_0 = s.Slice(4);
            if (s_0.StartsWith("altinn:test"))
            {
                var s_0_0 = s_0.Slice(11);
                if (s_0_0.StartsWith("1"))
                {
                    var s_0_0_0 = s_0_0.Slice(1);

                    if (s_0_0_0.Length > 1 && s_0_0_0[0] == ':' && TryParseTest1(s_0_0_0.Slice(1), provider, out System.Guid s_0_0_0_value))
                    {
                        result = Test1.FromParsed(original ?? new string(s), 17, s_0_0_0_value);
                        return true;
                    }

                    result = default;
                    return false;
                }

                if (s_0_0.StartsWith("2"))
                {
                    var s_0_0_1 = s_0_0.Slice(1);
                    if (s_0_0_1.StartsWith(":sub"))
                    {
                        var s_0_0_1_0 = s_0_0_1.Slice(4);
                        if (s_0_0_1_0.StartsWith("1"))
                        {
                            var s_0_0_1_0_0 = s_0_0_1_0.Slice(1);

                            if (s_0_0_1_0_0.Length > 1 && s_0_0_1_0_0[0] == ':' && TryParseTest2Sub(s_0_0_1_0_0.Slice(1), provider, out float s_0_0_1_0_0_value))
                            {
                                result = Test2Sub.FromParsed(original ?? new string(s), 22, s_0_0_1_0_0_value);
                                return true;
                            }

                            result = default;
                            return false;
                        }

                        if (s_0_0_1_0.StartsWith("2"))
                        {
                            var s_0_0_1_0_1 = s_0_0_1_0.Slice(1);

                            if (s_0_0_1_0_1.Length > 1 && s_0_0_1_0_1[0] == ':' && TryParseTest2Sub(s_0_0_1_0_1.Slice(1), provider, out float s_0_0_1_0_1_value))
                            {
                                result = Test2Sub.FromParsed(original ?? new string(s), 22, s_0_0_1_0_1_value);
                                return true;
                            }

                            result = default;
                            return false;
                        }


                        if (s_0_0_1_0.Length > 1 && s_0_0_1_0[0] == ':' && TryParseTest2Sub(s_0_0_1_0.Slice(1), provider, out float s_0_0_1_0_value))
                        {
                            result = Test2Sub.FromParsed(original ?? new string(s), 21, s_0_0_1_0_value);
                            return true;
                        }

                        result = default;
                        return false;
                    }


                    if (s_0_0_1.Length > 1 && s_0_0_1[0] == ':' && TryParseTest2(s_0_0_1.Slice(1), provider, out int s_0_0_1_value))
                    {
                        result = Test2.FromParsed(original ?? new string(s), 17, s_0_0_1_value);
                        return true;
                    }

                    result = default;
                    return false;
                }

                result = default;
                return false;
            }

            if (s_0.StartsWith("n"))
            {
                var s_0_1 = s_0.Slice(1);
                if (s_0_1.StartsWith("otaltinn"))
                {
                    var s_0_1_0 = s_0_1.Slice(8);

                    if (s_0_1_0.Length > 1 && s_0_1_0[0] == ':' && TryParseTest3(s_0_1_0.Slice(1), provider, out long s_0_1_0_value))
                    {
                        result = Test3.FromParsed(original ?? new string(s), 14, s_0_1_0_value);
                        return true;
                    }

                    result = default;
                    return false;
                }

                if (s_0_1.StartsWith("root"))
                {
                    var s_0_1_1 = s_0_1.Slice(4);

                    if (s_0_1_1.Length > 1 && s_0_1_1[0] == ':' && TryParseTest4(s_0_1_1.Slice(1), provider, out uint s_0_1_1_value))
                    {
                        result = Test4.FromParsed(original ?? new string(s), 10, s_0_1_1_value);
                        return true;
                    }

                    result = default;
                    return false;
                }

                result = default;
                return false;
            }

            result = default;
            return false;
        }

        result = default;
        return false;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out MyUrn result)
        => TryParse(s, provider, original: null, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out MyUrn result)
        => TryParse(s, provider: null, original: null, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static MyUrn Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, original: null, out MyUrn? result))
        {
            throw new FormatException("Could not parse MyUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static MyUrn Parse(ReadOnlySpan<char> s)
    {
        if (!TryParse(s, provider: null, original: null, out MyUrn? result))
        {
            throw new FormatException("Could not parse MyUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MyUrn result)
        => TryParse(s.AsSpan(), provider, original: s, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out MyUrn result)
        => TryParse(s.AsSpan(), provider: null, original: s, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static MyUrn Parse(string? s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (!TryParse(s.AsSpan(), provider, original: s, out MyUrn? result))
        {
            throw new FormatException("Could not parse MyUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static MyUrn Parse(string? s)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (!TryParse(s.AsSpan(), provider: null, original: s, out MyUrn? result))
        {
            throw new FormatException("Could not parse MyUrn");
        }

        return result;
    }

    [CompilerGenerated]
    private static T Unreachable<T>() => throw new UnreachableException();

    [CompilerGenerated]
    private static bool TryAppend(Span<char> destination, out int charsWritten, ReadOnlySpan<char> source)
    {
    if (source.Length > destination.Length)
    {
        charsWritten = 0;
        return false;
    }

    source.CopyTo(destination);
    charsWritten = source.Length;
    return true;
    }

    [CompilerGenerated]
    public enum Type
    {
        Test1 = 1,
        Test2 = 2,
        Test2Sub = 3,
        Test3 = 4,
        Test4 = 5,
    }

    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed partial record Test1 : MyUrn
    {
        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:altinn:test1",
        ];

        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly System.Guid _value;

        [CompilerGenerated]
        private Test1(string urn, int valueIndex, System.Guid value) : base(urn, valueIndex, Type.Test1) => (_value) = (value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static Test1 FromParsed(string urn, int valueIndex, System.Guid value) => new(urn, valueIndex, value);

        [CompilerGenerated]
        public System.Guid Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(Test1? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Test1 Create(System.Guid value)
            => new($"""urn:altinn:test1:{new _FormatHelper(value)}""", 17, value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
            , ISpanFormattable
        {
            private readonly System.Guid _value;

            public _FormatHelper(System.Guid value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => TryFormatTest1(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatTest1(_value, format, provider);
        }
    }

    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed partial record Test2 : MyUrn
    {
        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:altinn:test2",
        ];

        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly int _value;

        [CompilerGenerated]
        private Test2(string urn, int valueIndex, int value) : base(urn, valueIndex, Type.Test2) => (_value) = (value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static Test2 FromParsed(string urn, int valueIndex, int value) => new(urn, valueIndex, value);

        [CompilerGenerated]
        public int Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(Test2? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Test2 Create(int value)
            => new($"""urn:altinn:test2:{new _FormatHelper(value)}""", 17, value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
            , ISpanFormattable
        {
            private readonly int _value;

            public _FormatHelper(int value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => TryFormatTest2(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatTest2(_value, format, provider);
        }
    }

    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed partial record Test2Sub : MyUrn
    {
        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:altinn:test2:sub",
            "urn:altinn:test2:sub1",
            "urn:altinn:test2:sub2",
        ];

        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly float _value;

        [CompilerGenerated]
        private Test2Sub(string urn, int valueIndex, float value) : base(urn, valueIndex, Type.Test2Sub) => (_value) = (value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static Test2Sub FromParsed(string urn, int valueIndex, float value) => new(urn, valueIndex, value);

        [CompilerGenerated]
        public float Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(Test2Sub? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Test2Sub Create(float value)
            => new($"""urn:altinn:test2:sub:{new _FormatHelper(value)}""", 21, value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
            , ISpanFormattable
        {
            private readonly float _value;

            public _FormatHelper(float value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => TryFormatTest2Sub(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatTest2Sub(_value, format, provider);
        }
    }

    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed partial record Test3 : MyUrn
    {
        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:notaltinn",
        ];

        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly long _value;

        [CompilerGenerated]
        private Test3(string urn, int valueIndex, long value) : base(urn, valueIndex, Type.Test3) => (_value) = (value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static Test3 FromParsed(string urn, int valueIndex, long value) => new(urn, valueIndex, value);

        [CompilerGenerated]
        public long Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(Test3? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Test3 Create(long value)
            => new($"""urn:notaltinn:{new _FormatHelper(value)}""", 14, value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
            , ISpanFormattable
        {
            private readonly long _value;

            public _FormatHelper(long value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => TryFormatTest3(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatTest3(_value, format, provider);
        }
    }

    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed partial record Test4 : MyUrn
    {
        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:nroot",
        ];

        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly uint _value;

        [CompilerGenerated]
        private Test4(string urn, int valueIndex, uint value) : base(urn, valueIndex, Type.Test4) => (_value) = (value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static Test4 FromParsed(string urn, int valueIndex, uint value) => new(urn, valueIndex, value);

        [CompilerGenerated]
        public uint Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(Test4? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Test4 Create(uint value)
            => new($"""urn:nroot:{new _FormatHelper(value)}""", 10, value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
            , ISpanFormattable
        {
            private readonly uint _value;

            public _FormatHelper(uint value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => TryFormatTest4(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatTest4(_value, format, provider);
        }
    }
}
