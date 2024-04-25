﻿//HintName: My.Test.Namespace.TestUrn.g.cs
#nullable enable

using Altinn.Urn;
using Altinn.Urn.Visit;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace My.Test.Namespace;

[DebuggerDisplay("{DebuggerDisplay}")]
[System.Text.Json.Serialization.JsonConverterAttribute(typeof(Altinn.Urn.Json.UrnJsonConverter<My.Test.Namespace.TestUrn>))]
partial record TestUrn
    : IParsable<TestUrn>
    , ISpanParsable<TestUrn>
    , IFormattable
    , ISpanFormattable
    , IKeyValueUrn<TestUrn, TestUrn.Type>
    , IVisitableKeyValueUrn
{
    private static readonly ImmutableArray<Type> _variants = [
        Type.Test2,
    ];

    private static readonly ImmutableArray<string> _validPrefixes = [
        "urn:altinn:test1",
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
            Type.Test2 => Test2.Prefixes,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string CanonicalPrefixFor(Type type)
        => type switch {
            Type.Test2 => Test2.CanonicalPrefix,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type ValueTypeFor(Type type)
        => type switch {
            Type.Test2 => typeof(System.Guid),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type VariantTypeFor(Type type)
        => type switch {
            Type.Test2 => typeof(Test2),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string NameFor(Type type)
        => type switch {
            Type.Test2 => nameof(Test2),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    private readonly KeyValueUrn _urn;
    private readonly Type _type;

    [CompilerGenerated]
    private TestUrn(string urn, int valueIndex, Type type) => (_urn, _type) = (KeyValueUrn.CreateUnchecked(urn, valueIndex), type);

    /// <inheritdoc/>
    [CompilerGenerated]
    public string Urn => _urn.Urn;

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
    public ReadOnlySpan<char> ValueSpan => _urn.ValueSpan;

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlyMemory<char> ValueMemory => _urn.ValueMemory;

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlySpan<char> KeySpan => _urn.KeySpan;

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlyMemory<char> KeyMemory => _urn.KeyMemory;

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlySpan<char> PrefixSpan => _urn.PrefixSpan;

    /// <inheritdoc/>
    [CompilerGenerated]
    public ReadOnlyMemory<char> PrefixMemory => _urn.PrefixMemory;

    /// <inheritdoc/>
    [CompilerGenerated]
    public string ToString(string? format, IFormatProvider? provider)
    {
        switch (format.AsSpan())
        {
            case ['V', ..var valueFormatSpan]:
                var valueFormat = valueFormatSpan.Length == 0 ? null : new string(valueFormatSpan);
                return _type switch
                {
                    Type.Test2 => FormatTest2(((Test2)this).Value, valueFormat, provider),
                    _ => Unreachable<string>(),
                };

            default:
                return _urn.ToString(format, provider);
        }
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        switch (format)
        {
            case ['V', ..var valueFormatSpan]:
                charsWritten = 0;
                return _type switch
                {
                    Type.Test2 => TryFormatTest2(((Test2)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    _ => Unreachable<bool>(),
                };

            default:
                return _urn.TryFormat(destination, out charsWritten, format, provider);
        }
    }

    [CompilerGenerated]
    protected abstract void Accept(IKeyValueUrnVisitor visitor);

    /// <inheritdoc/>
    [CompilerGenerated]
    void IVisitableKeyValueUrn.Accept(IKeyValueUrnVisitor visitor) => Accept(visitor);

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsTest2([MaybeNullWhen(false)] out System.Guid value)
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

    /// <inheritdoc/>
    [CompilerGenerated]
    public override string ToString() => _urn.Urn;

    [CompilerGenerated]
    protected string DebuggerDisplay => _urn.Urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public virtual bool Equals(TestUrn? other) => other is not null && _type == other._type && _urn == other._urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public override int GetHashCode() => _urn.GetHashCode();

    [CompilerGenerated]
    private static bool TryParseTest2(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out System.Guid value)
        => System.Guid.TryParse(segment, provider, out value);

    [CompilerGenerated]
    private static string FormatTest2(System.Guid value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    [CompilerGenerated]
    private static bool TryFormatTest2(System.Guid value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryGetVariant(ReadOnlySpan<char> prefix, [MaybeNullWhen(returnValue: false)] out Type variant)
    {
        ReadOnlySpan<char> s = prefix;
        if (s.StartsWith("urn:altinn:test1"))
        {
            var s_0 = s.Slice(16);

            if (s_0.Length == 0)
            {
                variant = Type.Test2;
                return true;
            }

            variant = default;
            return false;
        }

        variant = default;
        return false;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryGetVariant(string prefix, [MaybeNullWhen(returnValue: false)] out Type variant)
        => TryGetVariant(prefix.AsSpan(), out variant);

    [CompilerGenerated]
    private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, string? original, [MaybeNullWhen(false)] out TestUrn result)
    {
        if (s.StartsWith("urn:altinn:test1"))
        {
            var s_0 = s.Slice(16);

            if (s_0.Length > 1 && s_0[0] == ':' && TryParseTest2(s_0.Slice(1), provider, out System.Guid s_0_value))
            {
                result = Test2.FromParsed(original ?? new string(s), 17, s_0_value);
                return true;
            }

            result = default;
            return false;
        }

        result = default;
        return false;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out TestUrn result)
        => TryParse(s, provider, original: null, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out TestUrn result)
        => TryParse(s, provider: null, original: null, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static TestUrn Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, original: null, out TestUrn? result))
        {
            throw new FormatException("Could not parse TestUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static TestUrn Parse(ReadOnlySpan<char> s)
    {
        if (!TryParse(s, provider: null, original: null, out TestUrn? result))
        {
            throw new FormatException("Could not parse TestUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TestUrn result)
        => TryParse(s.AsSpan(), provider, original: s, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out TestUrn result)
        => TryParse(s.AsSpan(), provider: null, original: s, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static TestUrn Parse(string? s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (!TryParse(s.AsSpan(), provider, original: s, out TestUrn? result))
        {
            throw new FormatException("Could not parse TestUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static TestUrn Parse(string? s)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (!TryParse(s.AsSpan(), provider: null, original: s, out TestUrn? result))
        {
            throw new FormatException("Could not parse TestUrn");
        }

        return result;
    }

    [CompilerGenerated]
    private static T Unreachable<T>() => throw new UnreachableException();

    [CompilerGenerated]
    public enum Type
    {
        Test2 = 1,
    }

    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    [System.Text.Json.Serialization.JsonConverterAttribute(typeof(Altinn.Urn.Json.UrnVariantJsonConverterFactory<My.Test.Namespace.TestUrn, My.Test.Namespace.TestUrn.Type>))]
    public sealed partial record Test2
        : TestUrn
        , IKeyValueUrnVariant<Test2, TestUrn, Type, System.Guid>
    {
        [CompilerGenerated]
        public const string CanonicalPrefix = "urn:altinn:test1";

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Type Variant => Type.Test2;

        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:altinn:test1",
        ];

        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly System.Guid _value;

        [CompilerGenerated]
        private Test2(string urn, int valueIndex, System.Guid value) : base(urn, valueIndex, Type.Test2) => (_value) = (value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static Test2 FromParsed(string urn, int valueIndex, System.Guid value) => new(urn, valueIndex, value);

        [CompilerGenerated]
        public System.Guid Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn.Urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(Test2? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Test2 Create(System.Guid value)
            => new($"""urn:altinn:test1:{new _FormatHelper(value)}""", 17, value);

        [CompilerGenerated]
        protected override void Accept(IKeyValueUrnVisitor visitor)
            => visitor.Visit<TestUrn, Type, System.Guid>(this, _type, _value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
            , ISpanFormattable
        {
            private readonly System.Guid _value;

            public _FormatHelper(System.Guid value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => TryFormatTest2(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatTest2(_value, format, provider);
        }
    }
}
