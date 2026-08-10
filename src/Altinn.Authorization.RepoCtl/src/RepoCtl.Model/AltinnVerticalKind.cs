using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.RepoCtl.Model.Utils;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents the kind of vertical in Altinn.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
[JsonConverter(typeof(AltinnVerticalKind.JsonConverter))]
public readonly record struct AltinnVerticalKind
    : IEquatable<AltinnVerticalKind>
    , IFormattable
    , ISpanFormattable
    , IParsable<AltinnVerticalKind>
    , ISpanParsable<AltinnVerticalKind>
    , IComparable<AltinnVerticalKind>
{
    /// <summary>
    /// Represents a deployable application.
    /// </summary>
    public static readonly AltinnVerticalKind Application = new(AltinnVerticalKindSet.Application);

    /// <summary>
    /// Represents an internal library that can be used by other libraries or applications.
    /// </summary>

    public static readonly AltinnVerticalKind Library = new(AltinnVerticalKindSet.Library);

    /// <summary>
    /// Represents a package that is published to a package registry. Can be used by all other verticals.
    /// </summary>
    public static readonly AltinnVerticalKind Package = new(AltinnVerticalKindSet.Package);

    /// <summary>
    /// Represents a tool (a special kind of package) that is published to a package registry. Cannot be used as a dependency.
    /// </summary>
    public static readonly AltinnVerticalKind Tool = new(AltinnVerticalKindSet.Tool);

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

        return result._value.Count == 1;
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

        return result._value.Count == 1;
    }

    internal readonly AltinnVerticalKindSet _value;

    internal AltinnVerticalKind(AltinnVerticalKindSet value)
    {
        Debug.Assert(value.Count == 1);
        _value = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance has a valid value.
    /// </summary>
    public bool HasValue => _value.Count == 1;

    /// <inheritdoc/>
    public override int GetHashCode()
        => _value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString()
        => _value.ToString();

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => format switch
        {
            "dir" => DirName(this),
            _ => _value.ToString(format, formatProvider),
        };

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => format switch
        {
            "dir" => DirName(this).TryCopyTo(destination, out charsWritten),
            _ => _value.TryFormat(destination, out charsWritten, format, provider),
        };

    /// <inheritdoc/>
    public int CompareTo(AltinnVerticalKind other)
        => AltinnVerticalKindSet.Compare(this, other);

    private static string DirName(AltinnVerticalKind kind)
    {
        if (kind == Application)
        {
            return "apps";
        }
        else if (kind == Library)
        {
            return "libs";
        }
        else if (kind == Package)
        {
            return "pkgs";
        }
        else if (kind == Tool)
        {
            return "tools";
        }
        else
        {
            throw new UnreachableException($"The value '{kind}' is not a valid AltinnVerticalKind.");
        }
    }

    internal sealed class JsonConverter
        : JsonConverter<AltinnVerticalKind>
    {
        public override AltinnVerticalKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value is null)
            {
                throw new JsonException("The value is null.");
            }

            if (!TryParse(value, null, out var result))
            {
                throw new JsonException($"The value '{value}' is not a valid AltinnVerticalKind.");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, AltinnVerticalKind value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
