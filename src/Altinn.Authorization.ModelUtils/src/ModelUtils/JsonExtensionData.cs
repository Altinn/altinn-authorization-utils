using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Represents additional JSON data that was present in the original JSON document but not mapped to any property in the model.
/// </summary>
[DebuggerDisplay("Count = {Count,nq}")]
public readonly struct JsonExtensionData
    : IEquatable<JsonExtensionData>
    , IEqualityOperators<JsonExtensionData, JsonExtensionData, bool>
    , IReadOnlyDictionary<string, JsonElement>
    , IEnumerable<JsonProperty>
{
    private readonly JsonElement _jsonElement;

    /// <inheritdoc/>
    IEnumerable<string> IReadOnlyDictionary<string, JsonElement>.Keys
        => ((IEnumerable<JsonProperty>)this).Select(static prop => prop.Name);

    /// <inheritdoc/>
    IEnumerable<JsonElement> IReadOnlyDictionary<string, JsonElement>.Values
        => ((IEnumerable<JsonProperty>)this).Select(static prop => prop.Value);

    /// <inheritdoc/>
    public int Count
        => _jsonElement.GetPropertyCount();

    /// <inheritdoc/>
    public JsonElement this[string key]
        => _jsonElement.GetProperty(key);

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonExtensionData"/> struct with the specified JSON element.
    /// </summary>
    /// <param name="jsonElement">The json element.</param>
    public JsonExtensionData(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Object)
        {
            ThrowHelper.ThrowArgumentException(nameof(jsonElement), "JsonExtensionData must be a JSON object.");
        }

        _jsonElement = jsonElement;
    }

    /// <inheritdoc/>
    public bool Equals(JsonExtensionData other)
    {
        if (_jsonElement.ValueKind != other._jsonElement.ValueKind)
        {
            return false;
        }

        if (_jsonElement.ValueKind != JsonValueKind.Object)
        {
            // both should be undefined
            return true;
        }

        return JsonElement.DeepEquals(this, other);
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is JsonExtensionData other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_jsonElement.ValueKind != JsonValueKind.Object)
        {
            // undefined
            return 0;
        }

        var hash = new HashCode();
        var inner = 0;

        // note: hash-code computation should be cheap, and as such, we don't descend into the JSON properties deeply.
        foreach (var prop in this)
        {
            var propHash = HashCode.Combine(StringComparer.Ordinal.GetHashCode(prop.Name), prop.Value.ValueKind);
            inner ^= propHash; // XOR combine property hash codes to ensure order independence
        }

        hash.Add(inner);
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override string ToString()
        => _jsonElement.ToString();

    /// <inheritdoc/>
    public JsonElement.ObjectEnumerator GetEnumerator()
    {
        if (_jsonElement.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        return _jsonElement.EnumerateObject();
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    IEnumerator<JsonProperty> IEnumerable<JsonProperty>.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc/>
    public bool ContainsKey(string key)
        => TryGetValue(key, out _);

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonElement value)
        => _jsonElement.TryGetProperty(key, out value);

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<string, JsonElement>> IEnumerable<KeyValuePair<string, JsonElement>>.GetEnumerator()
        => ((IEnumerable<JsonProperty>)this).Select(prop => KeyValuePair.Create(prop.Name, prop.Value)).GetEnumerator();

    /// <inheritdoc/>
    public static bool operator ==(JsonExtensionData left, JsonExtensionData right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(JsonExtensionData left, JsonExtensionData right)
        => !(left == right);

    /// <summary>
    /// Gets the underlying <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="extensionData">Self.</param>
    public static implicit operator JsonElement(JsonExtensionData extensionData)
        => extensionData._jsonElement;

    /// <summary>
    /// Casts a <see cref="JsonElement"/> to a <see cref="JsonExtensionData"/>.
    /// </summary>
    /// <param name="jsonElement">The json element.</param>
    /// <exception cref="ArgumentException">Thrown if the json element is not an object.</exception>
    public static explicit operator JsonExtensionData(JsonElement jsonElement)
        => new JsonExtensionData(jsonElement);
}
