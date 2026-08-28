using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.RepoCtl.Model.Utils;
using CommunityToolkit.Diagnostics;
using Slugify;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents the identifier of a vertical in Altinn.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
[JsonConverter(typeof(AltinnVerticalId.JsonConverter))]
public readonly record struct AltinnVerticalId
    : IEquatable<AltinnVerticalId>
    , IFormattable
    , ISpanFormattable
    , IParsable<AltinnVerticalId>
    , ISpanParsable<AltinnVerticalId>
    , IComparable<AltinnVerticalId>
{
    /// <inheritdoc/>
    public static AltinnVerticalId Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : ThrowHelper.ThrowFormatException<AltinnVerticalId>($"The value '{s}' is not a valid AltinnVerticalId.");

    /// <inheritdoc/>
    public static AltinnVerticalId Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : ThrowHelper.ThrowFormatException<AltinnVerticalId>($"The value '{s}' is not a valid AltinnVerticalId.");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AltinnVerticalId result)
        => TryParse(s.AsSpan(), provider, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out AltinnVerticalId result)
    {
        var separatorIndex = s.IndexOf(':');
        if (separatorIndex < 0 || separatorIndex > 5)
        {
            result = default;
            return false;
        }

        if (!AltinnVerticalKind.TryParse(s[..separatorIndex], provider, out var kind))
        {
            result = default;
            return false;
        }

        var name = s[(separatorIndex + 1)..];

        // TODO: validate name?
        result = new AltinnVerticalId(kind, name.ToString());
        return true;
    }

    private readonly AltinnVerticalKind _kind;
    private readonly string _name;

    /// <summary>
    /// Gets the vertical kind.
    /// </summary>
    public AltinnVerticalKind Kind => _kind;

    /// <summary>
    /// Gets the vertical name.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnVerticalId"/> struct.
    /// </summary>
    /// <param name="kind">The vertical kind.</param>
    /// <param name="name">The vertical name.</param>
    public AltinnVerticalId(AltinnVerticalKind kind, string name)
    {
        Guard.IsNotDefault(kind);
        Guard.IsNotNullOrEmpty(name);

        _kind = kind;
        _name = name;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = default;
        hash.Add(_kind);
        hash.Add(_name, StringComparer.Ordinal);

        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(AltinnVerticalId other)
        => _kind == other._kind
        && string.Equals(_name, other._name, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override string ToString()
        => $"{_kind}:{_name}";

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => format switch
        {
            "k" => _kind.ToString(),
            "n" => _name,
            "s" => Slugify(ToString()),
            "r" => $"{_kind}/{_name}",
            "" or null => ToString(),
            _ => ThrowHelper.ThrowFormatException<string>($"The format string '{format}' is not supported."),
        };

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;
        return format switch
        {
            "k" => _kind.TryFormat(destination, out charsWritten, "", provider),
            "n" => _name.AsSpan().TryCopyTo(destination, out charsWritten),
            "s" => ToString("s", provider).AsSpan().TryCopyTo(destination, out charsWritten),
            "r" => ToString("r", provider).AsSpan().TryCopyTo(destination, out charsWritten),
            "" => TryFormatInner(in this, destination, out charsWritten),
            _ => ThrowHelper.ThrowFormatException<bool>($"The format string '{format}' is not supported."),
        };

        static bool TryFormatInner(in AltinnVerticalId self, Span<char> destination, out int charsWritten)
        {
            if (!self._kind.TryFormat(destination, out charsWritten, "", provider: null))
            {
                return false;
            }

            if (charsWritten >= destination.Length)
            {
                return false;
            }

            destination[charsWritten++] = ':';
            destination = destination[charsWritten..];

            if (!self._name.TryCopyTo(destination, out var nameCharsWritten))
            {
                return false;
            }

            charsWritten += nameCharsWritten;
            return true;
        }
    }

    /// <inheritdoc/>
    public int CompareTo(AltinnVerticalId other)
    {
        var kindComparison = _kind.CompareTo(other._kind);
        if (kindComparison != 0)
        {
            return kindComparison;
        }

        return string.Compare(_name, other._name, StringComparison.Ordinal);
    }

    private static string Slugify(string value)
    {
        var builder = new SlugHelperForNonAsciiLanguages();
        builder.Config.StringReplacements["."] = "-";
        builder.Config.StringReplacements[":"] = "-";
        return builder.GenerateSlug(value);
    }

    internal sealed class JsonConverter
        : JsonConverter<AltinnVerticalId>
    {
        public override AltinnVerticalId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (stringValue is null)
            {
                throw new JsonException($"Expected an AltinnVerticalId, but got null.");
            }

            if (!AltinnVerticalId.TryParse(stringValue, provider: null, out var result))
            {
                throw new JsonException($"The value '{stringValue}' is not a valid AltinnVerticalId.");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, AltinnVerticalId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
