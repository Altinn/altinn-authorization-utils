//HintName: MyNamespace.AccessPackageUrn.g.cs
#nullable enable

using Altinn.Urn;
using Altinn.Urn.Visit;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MyNamespace;

[DebuggerDisplay("{DebuggerDisplay}")]
[System.Text.Json.Serialization.JsonConverterAttribute(typeof(Altinn.Urn.Json.UrnJsonConverter<MyNamespace.AccessPackageUrn>))]
partial record AccessPackageUrn
    : IParsable<AccessPackageUrn>
    , ISpanParsable<AccessPackageUrn>
    , IFormattable
    , ISpanFormattable
    , IKeyValueUrn<AccessPackageUrn, AccessPackageUrn.Type>
    , IVisitableKeyValueUrn
{
    private static readonly ImmutableArray<Type> _variants = [
        Type.AccessPackage,
    ];

    private static readonly ImmutableArray<string> _validPrefixes = [
        "urn:altinn:accesspackage",
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
            Type.AccessPackage => AccessPackage.Prefixes,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string CanonicalPrefixFor(Type type)
        => type switch {
            Type.AccessPackage => AccessPackage.CanonicalPrefix,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type ValueTypeFor(Type type)
        => type switch {
            Type.AccessPackage => typeof(MyNamespace.AccessPackageIdentifier),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type VariantTypeFor(Type type)
        => type switch {
            Type.AccessPackage => typeof(AccessPackage),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string NameFor(Type type)
        => type switch {
            Type.AccessPackage => nameof(AccessPackage),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    private readonly KeyValueUrn _urn;
    private readonly Type _type;

    [CompilerGenerated]
    private AccessPackageUrn(string urn, int valueIndex, Type type) => (_urn, _type) = (KeyValueUrn.CreateUnchecked(urn, valueIndex), type);

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
            case ['v']:
                return new string(ValueSpan);
            case ['k']:
                return new string(KeySpan);
            case ['V', ..var valueFormatSpan]:
                var valueFormat = valueFormatSpan.Length == 0 ? null : new string(valueFormatSpan);
                return _type switch
                {
                    Type.AccessPackage => FormatAccessPackage(((AccessPackage)this).Value, valueFormat, provider),
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
            case ['v']:
                charsWritten = ValueSpan.Length;
                return ValueSpan.TryCopyTo(destination);
            case ['k']:
                charsWritten = KeySpan.Length;
                return KeySpan.TryCopyTo(destination);
            case ['V', ..var valueFormatSpan]:
                charsWritten = 0;
                return _type switch
                {
                    Type.AccessPackage => TryFormatAccessPackage(((AccessPackage)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    _ => Unreachable<bool>(),
                };

            default:
                return _urn.TryFormat(destination, out charsWritten, format, provider);
        }
    }

    /// <inheritdoc cref="IVisitableKeyValueUrn.Accept(IKeyValueUrnVisitor)"/>
    [CompilerGenerated]
    protected abstract void Accept(IKeyValueUrnVisitor visitor);

    /// <inheritdoc/>
    [CompilerGenerated]
    void IVisitableKeyValueUrn.Accept(IKeyValueUrnVisitor visitor) => Accept(visitor);

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsAccessPackage([MaybeNullWhen(false)] out MyNamespace.AccessPackageIdentifier value)
    #pragma warning restore CS8826
    {
        if (_type == Type.AccessPackage)
        {
            value = ((AccessPackage)this).Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public override string ToString() => _urn.Urn;

    /// <summary>Gets the debugger display for this Urn.</summary>
    [CompilerGenerated]
    protected string DebuggerDisplay => _urn.Urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public virtual bool Equals(AccessPackageUrn? other) => other is not null && _type == other._type && _urn == other._urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public override int GetHashCode() => _urn.GetHashCode();

    [CompilerGenerated]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseAccessPackage(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out MyNamespace.AccessPackageIdentifier value)
    {
        return Impl<MyNamespace.AccessPackageIdentifier>(segment, provider, out value);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Impl<T>(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out T value)
            where T : IParsable<T>
            => T.TryParse(new string(segment), provider, out value);
    }

    [CompilerGenerated]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatAccessPackage(MyNamespace.AccessPackageIdentifier value, string? format, IFormatProvider? provider)
    {
        return Impl<MyNamespace.AccessPackageIdentifier>(value, format, provider);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string Impl<T>(T value, string? format, IFormatProvider? provider)
            where T : IFormattable
            => value.ToString(format, provider);
    }

    [CompilerGenerated]
    private static bool TryFormatAccessPackage(MyNamespace.AccessPackageIdentifier value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        string? formatString = format.Length == 0 ? null : new string(format);
        string formatted = FormatAccessPackage(value, formatString, provider);
        bool result = formatted.TryCopyTo(destination);
        charsWritten = result ? formatted.Length : 0;
        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryGetVariant(ReadOnlySpan<char> prefix, [MaybeNullWhen(returnValue: false)] out Type variant)
    {
        ReadOnlySpan<char> s = prefix;
        if (s.StartsWith("urn:altinn:accesspackage"))
        {
            var s_0 = s.Slice(24);

            if (s_0.Length == 0)
            {
                variant = Type.AccessPackage;
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
    private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, string? original, [MaybeNullWhen(false)] out AccessPackageUrn result)
    {
        if (s.StartsWith("urn:altinn:accesspackage"))
        {
            var s_0 = s.Slice(24);

            if (s_0.Length > 1 && s_0[0] == ':' && TryParseAccessPackage(s_0.Slice(1), provider, out MyNamespace.AccessPackageIdentifier? s_0_value))
            {
                result = AccessPackage.FromParsed(original ?? new string(s), 25, s_0_value);
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
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out AccessPackageUrn result)
        => TryParse(s, provider, original: null, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out AccessPackageUrn result)
        => TryParse(s, provider: null, original: null, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static AccessPackageUrn Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, original: null, out AccessPackageUrn? result))
        {
            throw new FormatException("Could not parse AccessPackageUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static AccessPackageUrn Parse(ReadOnlySpan<char> s)
    {
        if (!TryParse(s, provider: null, original: null, out AccessPackageUrn? result))
        {
            throw new FormatException("Could not parse AccessPackageUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AccessPackageUrn result)
        => TryParse(s.AsSpan(), provider, original: s, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out AccessPackageUrn result)
        => TryParse(s.AsSpan(), provider: null, original: s, out result);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static AccessPackageUrn Parse(string? s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (!TryParse(s.AsSpan(), provider, original: s, out AccessPackageUrn? result))
        {
            throw new FormatException("Could not parse AccessPackageUrn");
        }

        return result;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    public static AccessPackageUrn Parse(string? s)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (!TryParse(s.AsSpan(), provider: null, original: s, out AccessPackageUrn? result))
        {
            throw new FormatException("Could not parse AccessPackageUrn");
        }

        return result;
    }

    [CompilerGenerated]
    private static T Unreachable<T>() => throw new UnreachableException();

    /// <summary>Type of <see cref="AccessPackageUrn" />.</summary>
    [CompilerGenerated]
    public enum Type
    {
        /// <summary>Urn is a <see cref="AccessPackageUrn.AccessPackage" />.</summary>
        AccessPackage = 1,
    }

    /// <summary>A AccessPackage variant of <see cref="AccessPackageUrn"/>.</summary>
    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    [System.Text.Json.Serialization.JsonConverterAttribute(typeof(Altinn.Urn.Json.UrnVariantJsonConverterFactory<MyNamespace.AccessPackageUrn, MyNamespace.AccessPackageUrn.Type>))]
    public sealed partial record AccessPackage
        : AccessPackageUrn
        , IKeyValueUrnVariant<AccessPackage, AccessPackageUrn, Type, MyNamespace.AccessPackageIdentifier>
        , IParsable<AccessPackage>
        , ISpanParsable<AccessPackage>
    {
        /// <inheritdoc/>
        [CompilerGenerated]
        public const string CanonicalPrefix = "urn:altinn:accesspackage";

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Type Variant => Type.AccessPackage;

        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:altinn:accesspackage",
        ];

        /// <inheritdoc/>
        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly MyNamespace.AccessPackageIdentifier _value;

        [CompilerGenerated]
        private AccessPackage(string urn, int valueIndex, MyNamespace.AccessPackageIdentifier value) : base(urn, valueIndex, Type.AccessPackage) => (_value) = (value);

        /// <summary>Constructs a <see cref="AccessPackage"/> from parsed components.</summary>
        /// <param name="urn">The raw URN.</param>
        /// <param name="valueIndex">The index of the value in the URN.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns>A <see cref="AccessPackage"/> constructed from it's parts.</returns>
        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static AccessPackage FromParsed(string urn, int valueIndex, MyNamespace.AccessPackageIdentifier value) => new(urn, valueIndex, value);

        /// <inheritdoc/>
        [CompilerGenerated]
        public MyNamespace.AccessPackageIdentifier Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn.Urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(AccessPackage? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static AccessPackage Create(MyNamespace.AccessPackageIdentifier value)
            => new($"""urn:altinn:accesspackage:{new _FormatHelper(value)}""", 25, value);

        [CompilerGenerated]
        private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, string? original, [MaybeNullWhen(false)] out AccessPackage result)
        {
            if (s.StartsWith("urn:altinn:accesspackage"))
            {
                var s_0 = s.Slice(24);

                if (s_0.Length > 1 && s_0[0] == ':' && TryParseAccessPackage(s_0.Slice(1), provider, out MyNamespace.AccessPackageIdentifier? s_0_value))
                {
                    result = AccessPackage.FromParsed(original ?? new string(s), 25, s_0_value);
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
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out AccessPackage result)
            => TryParse(s, provider, original: null, out result);

        /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out TSelf)"/>
        [CompilerGenerated]
        public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out AccessPackage result)
            => TryParse(s, provider: null, original: null, out result);

        /// <inheritdoc/>
        [CompilerGenerated]
        public static new AccessPackage Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            if (!TryParse(s, provider, original: null, out AccessPackage? result))
            {
                throw new FormatException("Could not parse AccessPackage");
            }

            return result;
        }

        /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
        [CompilerGenerated]
        public static new AccessPackage Parse(ReadOnlySpan<char> s)
        {
            if (!TryParse(s, provider: null, original: null, out AccessPackage? result))
            {
                throw new FormatException("Could not parse AccessPackage");
            }

            return result;
        }

        /// <inheritdoc/>
        [CompilerGenerated]
        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AccessPackage result)
            => TryParse(s.AsSpan(), provider, original: s, out result);

        /// <inheritdoc cref="IParsable{TSelf}.TryParse(string, IFormatProvider?, out TSelf)"/>
        [CompilerGenerated]
        public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out AccessPackage result)
            => TryParse(s.AsSpan(), provider: null, original: s, out result);

        /// <inheritdoc/>
        [CompilerGenerated]
        public static new AccessPackage Parse(string? s, IFormatProvider? provider)
        {
            ArgumentNullException.ThrowIfNull(s);

            if (!TryParse(s.AsSpan(), provider, original: s, out AccessPackage? result))
            {
                throw new FormatException("Could not parse AccessPackage");
            }

            return result;
        }

        /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
        [CompilerGenerated]
        public static new AccessPackage Parse(string? s)
        {
            ArgumentNullException.ThrowIfNull(s);

            if (!TryParse(s.AsSpan(), provider: null, original: s, out AccessPackage? result))
            {
                throw new FormatException("Could not parse AccessPackage");
            }

            return result;
        }

        /// <inheritdoc/>
        [CompilerGenerated]
        protected override void Accept(IKeyValueUrnVisitor visitor)
            => visitor.Visit<AccessPackageUrn, Type, MyNamespace.AccessPackageIdentifier>(this, _type, _value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
            , ISpanFormattable
        {
            private readonly MyNamespace.AccessPackageIdentifier _value;

            public _FormatHelper(MyNamespace.AccessPackageIdentifier value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => TryFormatAccessPackage(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatAccessPackage(_value, format, provider);
        }
    }
}
