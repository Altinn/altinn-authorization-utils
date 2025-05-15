using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

/// <summary>
/// Helper for being able to call read and write on a JsonConverter without knowing the type of the converter.
/// </summary>
public interface IGenericJsonConverter
{
    /// <inheritdoc cref="JsonConverter{T}.Write(Utf8JsonWriter, T, JsonSerializerOptions)"/>
    public void WriteGeneric<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        where T : class;

    /// <inheritdoc cref="JsonConverter{T}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>
    public T? ReadGeneric<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        where T : class;
}
