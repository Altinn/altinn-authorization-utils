using System.Text.Json;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Extensions for <see cref="Utf8JsonReader"/>.
/// </summary>
public static class Utf8JsonReaderExtensions
{
    /// <summary>
    /// Skips the current token in the JSON reader in a manner that is safe when dealing with partial content.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/>.</param>
    /// <exception cref="JsonException">Thrown if skipping was not possible.</exception>
    public static void SafeSkip(this ref Utf8JsonReader reader)
    {
        if (!reader.TrySkip())
        {
            throw new JsonException("Failed to skip the current token in the JSON reader.");
        }
    }
}
