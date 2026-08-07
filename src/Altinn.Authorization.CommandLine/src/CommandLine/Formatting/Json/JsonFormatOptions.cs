using System.Text.Encodings.Web;
using System.Text.Json;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

/// <summary>
/// Options to configure JSON serialization settings for <see cref="JsonFormat"/>.
/// </summary>
public sealed class JsonFormatOptions
{
    internal static readonly JsonSerializerOptions DefaultSerializerOptions = CreateDefaultOptions();

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/>.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; }
        = DefaultSerializerOptions;

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Web)
        {
            // Allow trailing commas in JSON input when deserializing.
            AllowTrailingCommas = true,

            // Web defaults don't use the relaxed JSON escaping encoder.
            //
            // Because these options are for producing content that is written directly to the console
            // (and not embedded in an HTML page for example), we can use UnsafeRelaxedJsonEscaping.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        options.MakeReadOnly();
        return options;
    }
}
