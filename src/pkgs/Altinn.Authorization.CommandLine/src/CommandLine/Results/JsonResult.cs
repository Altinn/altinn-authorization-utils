using System.Buffers;
using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Json;
using Nerdbank.Streams;

namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Represents a command result that contains JSON data, which can be written to the console.
/// </summary>
public sealed class JsonResult
    : ICommandResult
    , IDisposable
{
    /// <summary>
    /// Creates a <see cref="JsonResult"/> from the specified <see cref="JsonDocument"/>.
    /// </summary>
    /// <param name="document">The JSON document.</param>
    /// <returns>A <see cref="JsonResult"/> representing the JSON document.</returns>
    public static JsonResult From(JsonDocument document)
        => From(document.RootElement);

    /// <summary>
    /// Creates a <see cref="JsonResult"/> from the specified <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="element">The JSON element.</param>
    /// <returns>A <see cref="JsonResult"/> representing the JSON element.</returns>
    public static JsonResult From(JsonElement element)
    {
        var json = new Sequence<byte>(ArrayPool<byte>.Shared);
        using var writer = new Utf8JsonWriter(json);
        element.WriteTo(writer);
        writer.Flush();

        return new JsonResult(json);
    }

    /// <summary>
    /// Creates a <see cref="JsonResult"/> from the specified JSON string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>A <see cref="JsonResult"/> representing the JSON string.</returns>
    public static JsonResult From(string json)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var sequence = new Sequence<byte>(ArrayPool<byte>.Shared);
        sequence.Write(bytes);

        return new JsonResult(sequence);
    }

    /// <summary>
    /// Creates a <see cref="JsonResult"/> from the specified value, serializing it to JSON using the provided options.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize to JSON.</param>
    /// <param name="options">The JSON serialization options.</param>
    /// <returns>A <see cref="JsonResult"/> representing the serialized JSON value.</returns>
    public static JsonResult Create<T>(T value, JsonSerializerOptions? options = null)
    {
        var json = new Sequence<byte>(ArrayPool<byte>.Shared);
        using var writer = new Utf8JsonWriter(json);
        JsonSerializer.Serialize(writer, value, options);
        writer.Flush();

        return new JsonResult(json);
    }

    private readonly Sequence<byte> _json;

    private JsonResult(Sequence<byte> json)
    {
        _json = json;
    }

    /// <inheritdoc/>
    public Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        context.Console.Write(JsonWithCommentsText.Create(_json.AsReadOnlySequence));
        context.Console.WriteAnsi(w => w.WriteLine());
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        ((IDisposable)_json).Dispose();
    }
}
