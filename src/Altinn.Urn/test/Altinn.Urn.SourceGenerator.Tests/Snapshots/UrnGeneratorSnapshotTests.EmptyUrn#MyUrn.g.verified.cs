﻿//HintName: MyUrn.g.cs
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

    private readonly RawUrn _urn;
    private readonly Type _type;

    [CompilerGenerated]
    private MyUrn(string urn, int valueIndex, Type type) => (_urn, _type) = (RawUrn.CreateUnchecked(urn, valueIndex), type);

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

    /// <inheritdoc/>
    [CompilerGenerated]
    public override string ToString() => _urn.Urn;

    [CompilerGenerated]
    protected string DebuggerDisplay => _urn.Urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public virtual bool Equals(MyUrn? other) => other is not null && _type == other._type && _urn == other._urn;

    /// <inheritdoc/>
    [CompilerGenerated]
    public override int GetHashCode() => _urn.GetHashCode();

    [CompilerGenerated]
    private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, string? original, [MaybeNullWhen(false)] out MyUrn result)
    {
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
    public enum Type
    {
    }
}
