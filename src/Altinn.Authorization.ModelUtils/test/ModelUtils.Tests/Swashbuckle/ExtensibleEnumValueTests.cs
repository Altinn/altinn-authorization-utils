using Altinn.Authorization.ModelUtils.Swashbuckle.OpenApi;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.Tests.Swashbuckle;

public class ExtensibleEnumOpenApiExtensionTests
{
    [Fact]
    public void WritesValuesAsArray()
    {
        var ext = new ExtensibleEnumOpenApiExtension();
        ext.Add(new ExtensibleEnumValue { Value = "value1" });
        ext.Add(new ExtensibleEnumValue { Value = "value2" });

        using var ms = new MemoryStream();
        {
            using var writer = new Utf8JsonWriter(ms);
            ((IOpenApiExtension)ext).Write(new JsonDocWriter(writer), OpenApiSpecVersion.OpenApi3_0);
        }

        ms.Position = 0;
        using var doc = JsonDocument.Parse(ms);

        doc.RootElement.ValueKind.ShouldBe(JsonValueKind.Array);
        var array = doc.RootElement.EnumerateArray().ToArray();

        array.Length.ShouldBe(2);
        array[0].GetProperty("value").GetString().ShouldBe("value1");
        array[1].GetProperty("value").GetString().ShouldBe("value2");
    }

    [Fact]
    public void ExtensibleEnumValue_WritesExpectedFields()
    {
        var value = new ExtensibleEnumValue
        {
            Value = "value",
            Title = "title",
            Description = "description",
            Deprecated = true,
            Preview = true,
        };

        using var ms = new MemoryStream();

        {
            using var writer = new Utf8JsonWriter(ms);
            ((IOpenApiExtension)value).Write(new JsonDocWriter(writer), OpenApiSpecVersion.OpenApi3_0);
        }

        ms.Position = 0;
        using var doc = JsonDocument.Parse(ms);

        doc.RootElement.ValueKind.ShouldBe(JsonValueKind.Object);
        var root = doc.RootElement;

        root.GetProperty("value").GetString().ShouldBe("value");
        root.GetProperty("title").GetString().ShouldBe("title");
        root.GetProperty("description").GetString().ShouldBe("description");
        root.GetProperty("deprecated").GetBoolean().ShouldBeTrue();
        root.GetProperty("preview").GetBoolean().ShouldBeTrue();
    }

    public class JsonDocWriter(Utf8JsonWriter writer)
        : IOpenApiWriter
    {
        public void Flush()
            => writer.Flush();

        public void WriteEndArray()
            => writer.WriteEndArray();

        public void WriteEndObject()
            => writer.WriteEndObject();

        public void WriteNull()
            => writer.WriteNullValue();

        public void WritePropertyName(string name)
            => writer.WritePropertyName(name);

        public void WriteRaw(string value)
            => writer.WriteStringValue(value);

        public void WriteStartArray()
            => writer.WriteStartArray();

        public void WriteStartObject()
            => writer.WriteStartObject();

        public void WriteValue(string value)
            => writer.WriteStringValue(value);

        public void WriteValue(decimal value)
            => writer.WriteNumberValue(value);

        public void WriteValue(int value)
            => writer.WriteNumberValue(value);

        public void WriteValue(bool value)
            => writer.WriteBooleanValue(value);

        public void WriteValue(object value)
            => JsonSerializer.Serialize(writer, value);
    }
}
