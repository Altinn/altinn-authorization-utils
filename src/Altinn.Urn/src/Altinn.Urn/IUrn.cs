using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn;

/// <summary>
/// Marker interface for URNs.
/// </summary>
public interface IUrn 
    : IFormattable
    , ISpanFormattable
{
    /// <summary>
    /// Gets the valid URN prefixes for the type.
    /// </summary>
    public static abstract ReadOnlySpan<string> Prefixes { get; }

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

public interface IUrn<TSelf>
    : IUrn
    , IParsable<TSelf>
    , ISpanParsable<TSelf>
    where TSelf : IUrn<TSelf>
{
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

public interface IUrn<TSelf, TVariants>
    : IUrn<TSelf>
    where TSelf : IUrn<TSelf, TVariants>
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
    /// Gets the URN type.
    /// </summary>
    public TVariants UrnType { get; }
}
