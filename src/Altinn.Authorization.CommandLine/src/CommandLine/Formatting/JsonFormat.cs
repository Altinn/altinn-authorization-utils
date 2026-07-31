using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Json;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Represents a JSON formatter that can format objects as JSON and write them to the console.
/// </summary>
public sealed class JsonFormat
    : IFormat
{
    private readonly IOptionsMonitor<JsonFormatOptions> _options;

    /// <inheritdoc/>
    public string Name => "json";

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFormat"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to configure JSON serialization settings.</param>
    public JsonFormat(IOptionsMonitor<JsonFormatOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public ValueTask Write<T>(T value, IFormatWriter writer, CancellationToken cancellationToken = default)
    {
        var options = _options.CurrentValue.SerializerOptions;
        var json = JsonSerializer.SerializeToUtf8Bytes(value, options);

        var jsonWidget = JsonWithCommentsText.Create(json, options);
        writer.Write(jsonWidget);
        writer.WriteAnsi(w => w.WriteLine());

        return ValueTask.CompletedTask;
    }
}
