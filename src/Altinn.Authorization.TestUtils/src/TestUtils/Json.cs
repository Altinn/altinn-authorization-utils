using System.Text.Json;

namespace Altinn.Authorization.TestUtils;

/// <summary>
/// Json serialization and deserialization utilities.
/// </summary>
public static class Json
{
    private readonly static JsonSerializerOptions _options = JsonSerializerOptions.Web;

    /// <summary>
    /// Get the default JSON serializer options used for serialization and deserialization.
    /// </summary>
    public static JsonSerializerOptions Options => _options;

    /// <summary>
    /// Serialize an object to a <see cref="JsonDocument"/> using the default options.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type.</param>
    /// <returns>A <see cref="JsonDocument"/>.</returns>
    public static JsonDocument SerializeToDocument(object? value, Type type)
        => JsonSerializer.SerializeToDocument(value, type, _options);

    /// <summary>
    /// Serialize an object to a <see cref="JsonDocument"/> using the default options.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="JsonDocument"/>.</returns>
    public static JsonDocument SerializeToDocument<T>(T value)
        => SerializeToDocument(value, typeof(T));

    /// <summary>
    /// Serialize an object to a JSON string using the default options.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type.</param>
    /// <returns>A JSON string.</returns>
    public static string SerializeToString(object? value, Type type)
        => JsonSerializer.Serialize(value, type, _options);

    /// <summary>
    /// Serialize an object to a JSON string using the default options.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A JSON string.</returns>
    public static string SerializeToString<T>(T value)
        => SerializeToString(value, typeof(T));

    /// <summary>
    /// Deserializes the specified <see cref="JsonDocument"/> into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="document">The <see cref="JsonDocument"/> to deserialize.</param>
    /// <returns>An object of type <typeparamref name="T"/> if the deserialization is successful; otherwise, <see
    /// langword="null"/>.</returns>
    public static T? Deserialize<T>(JsonDocument document)
        => JsonSerializer.Deserialize<T>(document, _options);

    /// <summary>
    /// Deserializes the specified <see cref="JsonDocument"/> into an object of the specified type.
    /// </summary>
    /// <param name="document">The <see cref="JsonDocument"/> to deserialize. Cannot be <see langword="null"/>.</param>
    /// <param name="type">The target <see cref="Type"/> to deserialize the JSON content into. Cannot be <see langword="null"/>.</param>
    /// <returns>An object of the specified type populated with the deserialized data, or <see langword="null"/> if the
    /// deserialization fails.</returns>
    public static object? Deserialize(JsonDocument document, Type type)
        => JsonSerializer.Deserialize(document, type, _options);

    /// <summary>
    /// Deserializes the specified JSON string into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="document">The JSON string to deserialize. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>An object of type <typeparamref name="T"/> that represents the deserialized JSON string,  or <see
    /// langword="null"/> if the input is invalid or the JSON string represents a null value.</returns>
    public static T? Deserialize<T>(string document)
        => JsonSerializer.Deserialize<T>(document, _options);

    /// <summary>
    /// Deserializes the specified JSON document into an object of the specified type.
    /// </summary>
    /// <param name="document">The JSON document to deserialize. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="type">The type of the object to deserialize the JSON document into. Cannot be <see langword="null"/>.</param>
    /// <returns>An object of the specified type populated with the data from the JSON document,  or <see langword="null"/> if
    /// the document is empty or cannot be deserialized.</returns>
    public static object? Deserialize(string document, Type type)
        => JsonSerializer.Deserialize(document, type, _options);
}
