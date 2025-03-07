using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.Tests.Utils;

public static class Json
{
#if NET9_0_OR_GREATER
    private readonly static JsonSerializerOptions _options = JsonSerializerOptions.Web;
#else
    private readonly static JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);
#endif

    public static JsonSerializerOptions Options => _options;

    public static JsonDocument SerializeToDocument(object? value, Type type)
        => JsonSerializer.SerializeToDocument(value, type, _options);

    public static JsonDocument SerializeToDocument<T>(T value)
        => SerializeToDocument(value, typeof(T));

    public static T? Deserialize<T>(JsonDocument document)
        => JsonSerializer.Deserialize<T>(document, _options);

    public static object? Deserialize(JsonDocument document, Type type)
        => JsonSerializer.Deserialize(document, type, _options);

    public static T? Deserialize<T>(string document)
        => JsonSerializer.Deserialize<T>(document, _options);

    public static object? Deserialize(string document, Type type)
        => JsonSerializer.Deserialize(document, type, _options);
}
