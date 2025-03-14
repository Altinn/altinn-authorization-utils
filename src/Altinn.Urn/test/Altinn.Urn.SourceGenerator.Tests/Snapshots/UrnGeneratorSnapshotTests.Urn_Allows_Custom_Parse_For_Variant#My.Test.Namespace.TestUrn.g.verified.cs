//HintName: My.Test.Namespace.TestUrn.g.cs
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
        Type.PartyId,
        Type.PartyUuid,
        Type.OrganizationIdentifier,
    ];

    private static readonly ImmutableArray<string> _validPrefixes = [
        "urn:altinn:party:id",
        "urn:altinn:party:uuid",
        "urn:altinn:organization:identifier-no",
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
            Type.PartyId => PartyId.Prefixes,
            Type.PartyUuid => PartyUuid.Prefixes,
            Type.OrganizationIdentifier => OrganizationIdentifier.Prefixes,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string CanonicalPrefixFor(Type type)
        => type switch {
            Type.PartyId => PartyId.CanonicalPrefix,
            Type.PartyUuid => PartyUuid.CanonicalPrefix,
            Type.OrganizationIdentifier => OrganizationIdentifier.CanonicalPrefix,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type ValueTypeFor(Type type)
        => type switch {
            Type.PartyId => typeof(int),
            Type.PartyUuid => typeof(System.Guid),
            Type.OrganizationIdentifier => typeof(OrganizationNumber),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type VariantTypeFor(Type type)
        => type switch {
            Type.PartyId => typeof(PartyId),
            Type.PartyUuid => typeof(PartyUuid),
            Type.OrganizationIdentifier => typeof(OrganizationIdentifier),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string NameFor(Type type)
        => type switch {
            Type.PartyId => nameof(PartyId),
            Type.PartyUuid => nameof(PartyUuid),
            Type.OrganizationIdentifier => nameof(OrganizationIdentifier),
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
                    Type.PartyId => FormatPartyId(((PartyId)this).Value, valueFormat, provider),
                    Type.PartyUuid => FormatPartyUuid(((PartyUuid)this).Value, valueFormat, provider),
                    Type.OrganizationIdentifier => FormatOrganizationIdentifier(((OrganizationIdentifier)this).Value, valueFormat, provider),
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
                    Type.PartyId => TryFormatPartyId(((PartyId)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    Type.PartyUuid => TryFormatPartyUuid(((PartyUuid)this).Value, destination, out charsWritten, valueFormatSpan, provider),
                    Type.OrganizationIdentifier => false,
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
    public partial bool IsPartyId([MaybeNullWhen(false)] out int value)
    #pragma warning restore CS8826
    {
        if (_type == Type.PartyId)
        {
            value = ((PartyId)this).Value;
            return true;
        }

        value = default;
        return false;
    }

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsPartyUuid([MaybeNullWhen(false)] out System.Guid value)
    #pragma warning restore CS8826
    {
        if (_type == Type.PartyUuid)
        {
            value = ((PartyUuid)this).Value;
            return true;
        }

        value = default;
        return false;
    }

    #pragma warning disable CS8826
    [CompilerGenerated]
    public partial bool IsOrganizationIdentifier([MaybeNullWhen(false)] out OrganizationNumber value)
    #pragma warning restore CS8826
    {
        if (_type == Type.OrganizationIdentifier)
        {
            value = ((OrganizationIdentifier)this).Value;
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
    public virtual bool Equals(TestUrn? other) => other is not null && _type == other._type && _urn == other._urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public override int GetHashCode() => _urn.GetHashCode();

    [CompilerGenerated]
    private static bool TryParsePartyUuid(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out System.Guid value)
        => System.Guid.TryParse(segment, provider, out value);

    [CompilerGenerated]
    private static bool TryParseOrganizationIdentifier(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out OrganizationNumber value)
        => OrganizationNumber.TryParse(segment, provider, out value);

    [CompilerGenerated]
    private static string FormatPartyId(int value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    [CompilerGenerated]
    private static bool TryFormatPartyId(int value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);

    [CompilerGenerated]
    private static string FormatPartyUuid(System.Guid value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    [CompilerGenerated]
    private static bool TryFormatPartyUuid(System.Guid value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);

    [CompilerGenerated]
    private static string FormatOrganizationIdentifier(OrganizationNumber value, string? format, IFormatProvider? provider)
        => (value as IFormattable).ToString(format, provider);

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryGetVariant(ReadOnlySpan<char> prefix, [MaybeNullWhen(returnValue: false)] out Type variant)
    {
        ReadOnlySpan<char> s = prefix;
        if (s.StartsWith("urn:altinn:"))
        {
            var s_0 = s.Slice(11);
            if (s_0.StartsWith("organization:identifier-no"))
            {
                var s_0_0 = s_0.Slice(26);

                if (s_0_0.Length == 0)
                {
                    variant = Type.OrganizationIdentifier;
                    return true;
                }

                variant = default;
                return false;
            }

            if (s_0.StartsWith("party:"))
            {
                var s_0_1 = s_0.Slice(6);
                if (s_0_1.StartsWith("id"))
                {
                    var s_0_1_0 = s_0_1.Slice(2);

                    if (s_0_1_0.Length == 0)
                    {
                        variant = Type.PartyId;
                        return true;
                    }

                    variant = default;
                    return false;
                }

                if (s_0_1.StartsWith("uuid"))
                {
                    var s_0_1_1 = s_0_1.Slice(4);

                    if (s_0_1_1.Length == 0)
                    {
                        variant = Type.PartyUuid;
                        return true;
                    }

                    variant = default;
                    return false;
                }

                variant = default;
                return false;
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
        if (s.StartsWith("urn:altinn:"))
        {
            var s_0 = s.Slice(11);
            if (s_0.StartsWith("organization:identifier-no"))
            {
                var s_0_0 = s_0.Slice(26);

                if (s_0_0.Length > 1 && s_0_0[0] == ':' && TryParseOrganizationIdentifier(s_0_0.Slice(1), provider, out OrganizationNumber? s_0_0_value))
                {
                    result = OrganizationIdentifier.FromParsed(original ?? new string(s), 38, s_0_0_value);
                    return true;
                }

                result = default;
                return false;
            }

            if (s_0.StartsWith("party:"))
            {
                var s_0_1 = s_0.Slice(6);
                if (s_0_1.StartsWith("id"))
                {
                    var s_0_1_0 = s_0_1.Slice(2);

                    if (s_0_1_0.Length > 1 && s_0_1_0[0] == ':' && TryParsePartyId(s_0_1_0.Slice(1), provider, out int s_0_1_0_value))
                    {
                        result = PartyId.FromParsed(original ?? new string(s), 20, s_0_1_0_value);
                        return true;
                    }

                    result = default;
                    return false;
                }

                if (s_0_1.StartsWith("uuid"))
                {
                    var s_0_1_1 = s_0_1.Slice(4);

                    if (s_0_1_1.Length > 1 && s_0_1_1[0] == ':' && TryParsePartyUuid(s_0_1_1.Slice(1), provider, out System.Guid s_0_1_1_value))
                    {
                        result = PartyUuid.FromParsed(original ?? new string(s), 22, s_0_1_1_value);
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

    /// <summary>Type of <see cref="TestUrn" />.</summary>
    [CompilerGenerated]
    public enum Type
    {
        /// <summary>Urn is a <see cref="TestUrn.PartyId" />.</summary>
        PartyId = 1,
        /// <summary>Urn is a <see cref="TestUrn.PartyUuid" />.</summary>
        PartyUuid = 2,
        /// <summary>Urn is a <see cref="TestUrn.OrganizationIdentifier" />.</summary>
        OrganizationIdentifier = 3,
    }

    /// <summary>A PartyId variant of <see cref="TestUrn"/>.</summary>
    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    [System.Text.Json.Serialization.JsonConverterAttribute(typeof(Altinn.Urn.Json.UrnVariantJsonConverterFactory<My.Test.Namespace.TestUrn, My.Test.Namespace.TestUrn.Type>))]
    public sealed partial record PartyId
        : TestUrn
        , IKeyValueUrnVariant<PartyId, TestUrn, Type, int>
    {
        /// <inheritdoc/>
        [CompilerGenerated]
        public const string CanonicalPrefix = "urn:altinn:party:id";

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Type Variant => Type.PartyId;

        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:altinn:party:id",
        ];

        /// <inheritdoc/>
        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly int _value;

        [CompilerGenerated]
        private PartyId(string urn, int valueIndex, int value) : base(urn, valueIndex, Type.PartyId) => (_value) = (value);

        /// <summary>Constructs a <see cref="PartyId"/> from parsed components.</summary>
        /// <param name="urn">The raw URN.</param>
        /// <param name="valueIndex">The index of the value in the URN.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns>A <see cref="PartyId"/> constructed from it's parts.</returns>
        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static PartyId FromParsed(string urn, int valueIndex, int value) => new(urn, valueIndex, value);

        /// <inheritdoc/>
        [CompilerGenerated]
        public int Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn.Urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(PartyId? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static PartyId Create(int value)
            => new($"""urn:altinn:party:id:{new _FormatHelper(value)}""", 20, value);

        /// <inheritdoc/>
        [CompilerGenerated]
        protected override void Accept(IKeyValueUrnVisitor visitor)
            => visitor.Visit<TestUrn, Type, int>(this, _type, _value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
            , ISpanFormattable
        {
            private readonly int _value;

            public _FormatHelper(int value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                => TryFormatPartyId(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatPartyId(_value, format, provider);
        }
    }

    /// <summary>A PartyUuid variant of <see cref="TestUrn"/>.</summary>
    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    [System.Text.Json.Serialization.JsonConverterAttribute(typeof(Altinn.Urn.Json.UrnVariantJsonConverterFactory<My.Test.Namespace.TestUrn, My.Test.Namespace.TestUrn.Type>))]
    public sealed partial record PartyUuid
        : TestUrn
        , IKeyValueUrnVariant<PartyUuid, TestUrn, Type, System.Guid>
    {
        /// <inheritdoc/>
        [CompilerGenerated]
        public const string CanonicalPrefix = "urn:altinn:party:uuid";

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Type Variant => Type.PartyUuid;

        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:altinn:party:uuid",
        ];

        /// <inheritdoc/>
        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly System.Guid _value;

        [CompilerGenerated]
        private PartyUuid(string urn, int valueIndex, System.Guid value) : base(urn, valueIndex, Type.PartyUuid) => (_value) = (value);

        /// <summary>Constructs a <see cref="PartyUuid"/> from parsed components.</summary>
        /// <param name="urn">The raw URN.</param>
        /// <param name="valueIndex">The index of the value in the URN.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns>A <see cref="PartyUuid"/> constructed from it's parts.</returns>
        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static PartyUuid FromParsed(string urn, int valueIndex, System.Guid value) => new(urn, valueIndex, value);

        /// <inheritdoc/>
        [CompilerGenerated]
        public System.Guid Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn.Urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(PartyUuid? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static PartyUuid Create(System.Guid value)
            => new($"""urn:altinn:party:uuid:{new _FormatHelper(value)}""", 22, value);

        /// <inheritdoc/>
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
                => TryFormatPartyUuid(_value, destination, out charsWritten, format, provider);

            public string ToString(string? format, IFormatProvider? provider)
                => FormatPartyUuid(_value, format, provider);
        }
    }

    /// <summary>A OrganizationIdentifier variant of <see cref="TestUrn"/>.</summary>
    [CompilerGenerated]
    [DebuggerDisplay("{DebuggerDisplay}")]
    [System.Text.Json.Serialization.JsonConverterAttribute(typeof(Altinn.Urn.Json.UrnVariantJsonConverterFactory<My.Test.Namespace.TestUrn, My.Test.Namespace.TestUrn.Type>))]
    public sealed partial record OrganizationIdentifier
        : TestUrn
        , IKeyValueUrnVariant<OrganizationIdentifier, TestUrn, Type, OrganizationNumber>
    {
        /// <inheritdoc/>
        [CompilerGenerated]
        public const string CanonicalPrefix = "urn:altinn:organization:identifier-no";

        /// <inheritdoc/>
        [CompilerGenerated]
        public static Type Variant => Type.OrganizationIdentifier;

        private static readonly new ImmutableArray<string> _validPrefixes = [
            "urn:altinn:organization:identifier-no",
        ];

        /// <inheritdoc/>
        [CompilerGenerated]
        public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();

        private readonly OrganizationNumber _value;

        [CompilerGenerated]
        private OrganizationIdentifier(string urn, int valueIndex, OrganizationNumber value) : base(urn, valueIndex, Type.OrganizationIdentifier) => (_value) = (value);

        /// <summary>Constructs a <see cref="OrganizationIdentifier"/> from parsed components.</summary>
        /// <param name="urn">The raw URN.</param>
        /// <param name="valueIndex">The index of the value in the URN.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns>A <see cref="OrganizationIdentifier"/> constructed from it's parts.</returns>
        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static OrganizationIdentifier FromParsed(string urn, int valueIndex, OrganizationNumber value) => new(urn, valueIndex, value);

        /// <inheritdoc/>
        [CompilerGenerated]
        public OrganizationNumber Value => _value;
        /// <inheritdoc/>
        [CompilerGenerated]
        public override string ToString() => _urn.Urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public bool Equals(OrganizationIdentifier? other) => other is not null && _urn == other._urn;

        /// <inheritdoc/>
        [CompilerGenerated]
        public override int GetHashCode() => _urn.GetHashCode();

        /// <inheritdoc/>
        [CompilerGenerated]
        public static OrganizationIdentifier Create(OrganizationNumber value)
            => new($"""urn:altinn:organization:identifier-no:{new _FormatHelper(value)}""", 38, value);

        /// <inheritdoc/>
        [CompilerGenerated]
        protected override void Accept(IKeyValueUrnVisitor visitor)
            => visitor.Visit<TestUrn, Type, OrganizationNumber>(this, _type, _value);

        [CompilerGenerated]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct _FormatHelper
            : IFormattable
        {
            private readonly OrganizationNumber _value;

            public _FormatHelper(OrganizationNumber value) => _value = value;

            public string ToString(string? format, IFormatProvider? provider)
                => FormatOrganizationIdentifier(_value, format, provider);
        }
    }
}
