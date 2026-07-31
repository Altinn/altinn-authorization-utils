using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal sealed class JsonWithCommentsText
    : IRenderable
{
    private static JsonReaderOptions ReaderOptions => new()
    {
        AllowTrailingCommas = true,
        AllowMultipleValues = false,
        CommentHandling = JsonCommentHandling.Allow,
    };

    public static JsonWithCommentsText Create(ref Utf8JsonReader reader, JsonSerializerOptions? options = null)
    {
        var node = JsonWithCommentsReader.Read(ref reader);
        return new JsonWithCommentsText(node, options);
    }
    public static JsonWithCommentsText Create(Memory<byte> json, JsonSerializerOptions? options = null)
    {
        var reader = new Utf8JsonReader(json.Span, ReaderOptions);
        return Create(ref reader, options);
    }

    public static JsonWithCommentsText Create(ReadOnlySequence<byte> json, JsonSerializerOptions? options = null)
    {
        var reader = new Utf8JsonReader(json, ReaderOptions);
        return Create(ref reader, options);
    }

    public static JsonWithCommentsText Create(string json, JsonSerializerOptions? options = null)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json), ReaderOptions);
        return Create(ref reader, options);
    }

    public static JsonWithCommentsText Create(JsonElement json, JsonSerializerOptions? options = null)
        => Create(json.GetRawText(), options);

    public static JsonWithCommentsText Create(JsonDocument json, JsonSerializerOptions? options = null)
        => Create(json.RootElement, options);

    private readonly JsonWithCommentsNode _node;
    private readonly JsonSerializerOptions _options;

    private IRenderable? _rendered;
    private CachedOptions _cachedOptions;

    private JsonWithCommentsText(
        JsonWithCommentsNode node,
        JsonSerializerOptions? options = null)
    {
        _node = node;
        _options = options ?? JsonFormatOptions.DefaultSerializerOptions;
    }

    Measurement IRenderable.Measure(RenderOptions options, int maxWidth)
    {
        var cachedOptions = CachedOptions.Create(options);
        return GetInner(in cachedOptions).Measure(options, maxWidth);
    }

    IEnumerable<Segment> IRenderable.Render(RenderOptions options, int maxWidth)
    {
        var cachedOptions = CachedOptions.Create(options);
        return GetInner(in cachedOptions).Render(options, maxWidth);
    }

    private IRenderable GetInner(in CachedOptions cachedOptions)
    {
        if (_rendered is null || !cachedOptions.Equals(_cachedOptions))
        {
            _cachedOptions = cachedOptions;
            _rendered = Build(in cachedOptions);
        }

        return _rendered;
    }

    private IRenderable Build(in CachedOptions cachedOptions)
    {
        var encoder = _options.Encoder ?? JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        var indentationSize = _options.IndentSize;

        if (!cachedOptions.Ansi)
        {
            // If ANSI is not supported, we assume that the output is being piped to a file or as input to
            // another program. In this case, we want to output the raw JSON without any formatting or colorization.
            // using var doc = JsonDocument.ParseValue(ref reader);
            // var text = JsonSerializer.Serialize(doc, _options);
            // return new Text(text);
            var bufferWriter = new ArrayBufferWriter<byte>();
            {
                using var jsonWriter = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions
                {
                    Encoder = encoder,
                    SkipValidation = true,
                });

                _node.Write(jsonWriter);
            }

            var jsonText = Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
            return new Text(jsonText);
        }

        return _node.ToNotation(encoder, checked((uint)indentationSize));
    }

    private readonly record struct CachedOptions
    {
        public required ColorSystem ColorSystem { get; init; }
        public required bool Ansi { get; init; }
        public required bool Unicode { get; init; }

        public static CachedOptions Create(RenderOptions options)
        {
            return new CachedOptions
            {
                ColorSystem = options.ColorSystem,
                Ansi = options.Ansi,
                Unicode = options.Unicode,
            };
        }
    }
}
