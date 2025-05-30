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
    ];

    private static readonly ImmutableArray<string> _validPrefixes = [
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
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string CanonicalPrefixFor(Type type)
        => type switch {
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type ValueTypeFor(Type type)
        => type switch {
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static System.Type VariantTypeFor(Type type)
        => type switch {
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    /// <inheritdoc/>
    [CompilerGenerated]
    public static string NameFor(Type type)
        => type switch {
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

    /// <inheritdoc/>
    [CompilerGenerated]
    public static bool TryGetVariant(ReadOnlySpan<char> prefix, [MaybeNullWhen(returnValue: false)] out Type variant)
    {
        ReadOnlySpan<char> s = prefix;
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
    }
}
