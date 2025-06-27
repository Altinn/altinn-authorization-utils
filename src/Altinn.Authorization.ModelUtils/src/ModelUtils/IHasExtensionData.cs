using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Represents a model that can have JSON extension data. This is useful in round-tripping foreign data that might update the model in the future without breaking existing code.
/// </summary>
/// <remarks>
/// This interface is generally intended to be implemented explicitly.
/// </remarks>
public interface IHasExtensionData
{
    /// <summary>
    /// Gets the JSON extension data associated with this model.
    /// </summary>
    /// <remarks>
    /// This should always be either a JSON object, or <c>default</c>. Always check that it's a JSON object before accessing properties on it.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public JsonElement JsonExtensionData { get; }

    /// <summary>
    /// Gets whether the model has JSON extension data or not.
    /// </summary>
    /// <returns><see langword="true"/> if the model has any JSON extension data, otherwise <see langword="false"/>.</returns>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool HasJsonExtensionData
        => JsonExtensionData.ValueKind == JsonValueKind.Object;

    /// <summary>
    /// Gets an enumerable/enumerator for the extension properties of the model.
    /// </summary>
    /// <returns>A <see cref="JsonElement.ObjectEnumerator"/> for JSON extension data in the model.</returns>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public JsonElement.ObjectEnumerator JsonExtensionProperties
    {
        get
        {
            var extensionData = JsonExtensionData;
            if (extensionData.ValueKind != JsonValueKind.Object)
            {
                return default;
            }

            return extensionData.EnumerateObject();
        }
    }
}
