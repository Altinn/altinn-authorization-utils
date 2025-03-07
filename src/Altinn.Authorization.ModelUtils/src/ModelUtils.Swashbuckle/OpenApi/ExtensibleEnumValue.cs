using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;

namespace Altinn.Authorization.ModelUtils.Swashbuckle.OpenApi;

internal sealed class ExtensibleEnumValue
    : IOpenApiExtension
{
    public required string Value { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }


    public bool Deprecated { get; set; }

    public bool Preview { get; set; }

    void IOpenApiExtension.Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        writer.WriteStartObject();

        if (!string.IsNullOrEmpty(Title))
        {
            writer.WriteProperty("value", Value);
        }

        if (!string.IsNullOrEmpty(Title))
        {
            writer.WriteProperty("title", Title);
        }

        if (!string.IsNullOrEmpty(Description))
        {
            writer.WriteProperty("description", Description);
        }

        if (Deprecated)
        {
            writer.WriteProperty("deprecated", Deprecated);
        }

        if (Preview)
        {
            writer.WriteProperty("preview", Preview);
        }

        writer.WriteEndObject();
    }
}
