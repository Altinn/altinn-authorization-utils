using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.RepoCtl.Model.Utils;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents the kind of vertical in Altinn.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public readonly record struct AltinnVerticalKind
    : IEquatable<AltinnVerticalKind>
    , IFormattable
    , ISpanFormattable
    , IParsable<AltinnVerticalKind>
    , ISpanParsable<AltinnVerticalKind>
{
    /// <inheritdoc cref="Kind.Application"/>
    public static readonly AltinnVerticalKind Application = new(Kind.Application);

    /// <inheritdoc cref="Kind.Library"/>
    public static readonly AltinnVerticalKind Library = new(Kind.Library);

    /// <inheritdoc cref="Kind.Package"/>
    public static readonly AltinnVerticalKind Package = new(Kind.Package);

    /// <inheritdoc cref="Kind.Tool"/>
    public static readonly AltinnVerticalKind Tool = new(Kind.Tool);

    /// <inheritdoc/>
    public static AltinnVerticalKind Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : ThrowHelper.ThrowFormatException<AltinnVerticalKind>($"The value '{s}' is not a valid AltinnVerticalKind.");

    /// <inheritdoc/>
    public static AltinnVerticalKind Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : ThrowHelper.ThrowFormatException<AltinnVerticalKind>($"The value '{s}' is not a valid AltinnVerticalKind.");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AltinnVerticalKind result)
    {
        result = s switch
        {
            "app" => Application,
            "lib" => Library,
            "pkg" => Package,
            "tool" => Tool,
            _ => default,
        };

        return result._kind != default;
    }

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out AltinnVerticalKind result)
    {
        result = s switch
        {
            "app" => Application,
            "lib" => Library,
            "pkg" => Package,
            "tool" => Tool,
            _ => default,
        };

        return result._kind != default;
    }

    private readonly Kind _kind;

    private AltinnVerticalKind(Kind kind)
    {
        _kind = kind;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
        => _kind.GetHashCode();

    /// <inheritdoc/>
    public override string ToString()
        => _kind switch
        {
            Kind.Application => "app",
            Kind.Library => "lib",
            Kind.Package => "pkg",
            Kind.Tool => "tool",
            _ => throw new InvalidOperationException($"Unknown kind: {_kind}"),
        };

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => ToString().TryCopyTo(destination, out charsWritten);


    /// <summary>
    /// The kind of vertical.
    /// </summary>
    private enum Kind
    {
        /// <summary>
        /// Represents a deployable application.
        /// </summary>
        Application = 1,

        /// <summary>
        /// Represents an internal library that can be used by other libraries or applications.
        /// </summary>
        Library = 2,

        /// <summary>
        /// Represents a package that is published to a package registry. Can be used by all other verticals.
        /// </summary>
        Package = 3,

        /// <summary>
        /// Represents a tool (a special kind of package) that is published to a package registry. Cannot be used as a dependency.
        /// </summary>
        Tool = 4,
    }
}
