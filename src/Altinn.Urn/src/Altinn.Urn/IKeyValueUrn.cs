﻿using Altinn.Urn.Visit;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn;

/// <summary>
/// An URN consisting of a key and a value.
/// </summary>
public interface IKeyValueUrn 
    : IFormattable
    , ISpanFormattable
{
    /// <summary>
    /// Gets the urn as a string.
    /// </summary>
    /// <remarks>This is the same as calling <see cref="object.ToString()"/>.</remarks>
    public string Urn { get; }

    /// <summary>
    /// Gets the URN as a readonly span of characters.
    /// </summary>
    /// <returns>The URN as a <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>s.</returns>
    /// <remarks>This is the same as calling <see cref="MemoryExtensions.AsSpan(string?)"/> on <see cref="Urn"/>.</remarks>
    public ReadOnlySpan<char> AsSpan();

    /// <summary>
    /// Gets the URN prefix as a span of characters.
    /// </summary>
    /// <remarks>
    /// This does not include the trailing colon.
    ///   <example>
    ///   For example:
    ///   <code>
    ///     var urn = MyUrn.Parse("urn:example:foo");
    ///     urn.PrefixSpan; // "urn:example"
    ///   </code>
    ///   </example>
    /// </remarks>
    public ReadOnlySpan<char> PrefixSpan { get; }

    /// <summary>
    /// Gets the URN key as a span of characters.
    /// </summary>
    /// <remarks>
    /// This does not include the trailing colon.
    ///   <example>
    ///   For example:
    ///   <code>
    ///     var urn = MyUrn.Parse("urn:example:foo");
    ///     urn.KeySpan; // "example"
    ///   </code>
    ///   </example>
    /// </remarks>
    public ReadOnlySpan<char> KeySpan { get; }

    /// <summary>
    /// Gets the URN value as a span of characters.
    /// </summary>
    /// <remarks>
    /// This does not include the leading colon.
    ///   <example>
    ///   For example:
    ///   <code>
    ///     var urn = MyUrn.Parse("urn:example:foo");
    ///     urn.ValueSpan; // "foo"
    ///   </code>
    ///   </example>
    /// </remarks>
    public ReadOnlySpan<char> ValueSpan { get; }

    /// <summary>
    /// Gets the URN as a readonly memory of characters.
    /// </summary>
    /// <returns>The URN as a <see cref="ReadOnlyMemory{T}"/> of <see langword="char"/>s.</returns>
    /// <remarks>This is the same as calling <see cref="MemoryExtensions.AsMemory(string?)"/> on <see cref="Urn"/>.</remarks>
    public ReadOnlyMemory<char> AsMemory();

    /// <summary>
    /// Gets the URN prefix as a memory of characters.
    /// </summary>
    /// <remarks>
    /// This does not include the trailing colon.
    ///   <example>
    ///   For example:
    ///   <code>
    ///     var urn = MyUrn.Parse("urn:example:foo");
    ///     urn.PrefixMemory; // "urn:example"
    ///   </code>
    ///   </example>
    /// </remarks>
    public ReadOnlyMemory<char> PrefixMemory { get; }

    /// <summary>
    /// Gets the URN key as a memory of characters.
    /// </summary>
    /// <remarks>
    /// This does not include the trailing colon.
    ///   <example>
    ///   For example:
    ///   <code>
    ///     var urn = MyUrn.Parse("urn:example:foo");
    ///     urn.KeyMemory; // "example"
    ///   </code>
    ///   </example>
    /// </remarks>
    public ReadOnlyMemory<char> KeyMemory { get; }

    /// <summary>
    /// Gets the URN value as a memory of characters.
    /// </summary>
    /// <remarks>
    /// This does not include the leading colon.
    ///   <example>
    ///   For example:
    ///   <code>
    ///     var urn = MyUrn.Parse("urn:example:foo");
    ///     urn.ValueMemory; // "foo"
    ///   </code>
    ///   </example>
    /// </remarks>
    public ReadOnlyMemory<char> ValueMemory { get; }
}

/// <summary>
/// An URN consisting of a key and a value.
/// </summary>
/// <typeparam name="TSelf">The type of the URN.</typeparam>
public interface IKeyValueUrn<TSelf>
    : IKeyValueUrn
    , IParsable<TSelf>
    , ISpanParsable<TSelf>
    where TSelf : IKeyValueUrn<TSelf>
{
    /// <summary>
    /// Gets the valid URN prefixes for the type.
    /// </summary>
    public static abstract ReadOnlySpan<string> Prefixes { get; }

    /// <summary>Parses a string into a value.</summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s" /> is <c>null</c>.</exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by <typeparamref name="TSelf" />.</exception>
    static abstract TSelf Parse(string s);

    /// <summary>Tries to parse a string into a value.</summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
    /// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
    static abstract bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(returnValue: false)] out TSelf result);

    /// <summary>Parses a span of characters into a value.</summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by <typeparamref name="TSelf" />.</exception>
    static abstract TSelf Parse(ReadOnlySpan<char> s);

    /// <summary>Tries to parse a span of characters into a value.</summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="result">On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
    /// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
    static abstract bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(returnValue: false)] out TSelf result);
}

/// <summary>
/// An URN consisting of a key and a value.
/// </summary>
/// <typeparam name="TSelf">The type of the URN.</typeparam>
/// <typeparam name="TVariants">The type of the enum representing the different variants this URN type supports.</typeparam>
public interface IKeyValueUrn<TSelf, TVariants>
    : IKeyValueUrn<TSelf>
    where TSelf : IKeyValueUrn<TSelf, TVariants>
    where TVariants : struct, Enum
{
    /// <summary>
    /// Gets the variants for the type.
    /// </summary>
    public static abstract ReadOnlySpan<TVariants> Variants { get; }

    /// <summary>
    /// Gets the name for a given variant.
    /// </summary>
    /// <param name="variant">The variant.</param>
    /// <returns>The name for the given variant.</returns>
    public static abstract string NameFor(TVariants variant);

    /// <summary>
    /// Gets the valid URN prefixes for a given variant.
    /// </summary>
    /// <param name="variant">The variant.</param>
    /// <returns>A set of valid prefixes for the given variant.</returns>
    public static abstract ReadOnlySpan<string> PrefixesFor(TVariants variant);

    /// <summary>
    /// Gets the canonical prefix for a given variant.
    /// </summary>
    /// <param name="variant">The variant.</param>
    /// <returns>The canonical prefix for the given variant.</returns>
    public static abstract string CanonicalPrefixFor(TVariants variant);

    /// <summary>
    /// Gets the variant type for a given variant.
    /// </summary>
    /// <param name="variant">The variant.</param>
    /// <returns>The variant type for the given variant.</returns>
    public static abstract Type VariantTypeFor(TVariants variant);

    /// <summary>
    /// Gets the value type for a given variant.
    /// </summary>
    /// <param name="variant">The variant.</param>
    /// <returns>The value type for the given variant.</returns>
    public static abstract Type ValueTypeFor(TVariants variant);

    /// <summary>
    /// Tries to get a variant from a prefix.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <param name="variant">The resulting variant.</param>
    /// <returns><see langword="true"/> if a variant with the given prefix was found, otherwise <see langword="false"/>.</returns>
    public static abstract bool TryGetVariant(ReadOnlySpan<char> prefix, [MaybeNullWhen(returnValue: false)] out TVariants variant);

    /// <summary>
    /// Tries to get a variant from a prefix.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <param name="variant">The resulting variant.</param>
    /// <returns><see langword="true"/> if a variant with the given prefix was found, otherwise <see langword="false"/>.</returns>
    public static abstract bool TryGetVariant(string prefix, [MaybeNullWhen(returnValue: false)] out TVariants variant);

    /// <summary>
    /// Gets the URN type.
    /// </summary>
    public TVariants UrnType { get; }
}

/// <summary>
/// An URN variant consisting of a key and a value.
/// </summary>
/// <typeparam name="TUrn">The type of the URN.</typeparam>
/// <typeparam name="TVariants">The type of the enum representing the different variants this URN type supports.</typeparam>
public interface IKeyValueUrnVariant<TUrn, TVariants>
    : IKeyValueUrn<TUrn, TVariants>
    , IVisitableKeyValueUrn
    where TUrn : IKeyValueUrn<TUrn, TVariants>
    where TVariants : struct, Enum
{
    /// <summary>
    /// Gets the variant of the URN.
    /// </summary>
    public static abstract TVariants Variant { get; }
}

/// <summary>
/// An URN variant consisting of a key and a value.
/// </summary>
/// <typeparam name="TSelf">The variant type.</typeparam>
/// <typeparam name="TUrn">The type of the URN.</typeparam>
/// <typeparam name="TVariants">The type of the enum representing the different variants this URN type supports.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public interface IKeyValueUrnVariant<TSelf, TUrn, TVariants, TValue>
    : IKeyValueUrnVariant<TUrn, TVariants>
    , IKeyValueUrn<TUrn, TVariants>
    where TUrn : IKeyValueUrn<TUrn, TVariants>
    where TVariants : struct, Enum
    where TSelf : TUrn, IKeyValueUrnVariant<TSelf, TUrn, TVariants, TValue>
{
    /// <summary>
    /// Gets the value of the URN.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Creates a new URN with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A new URN with the given value.</returns>
    public static abstract TSelf Create(TValue value);
}
