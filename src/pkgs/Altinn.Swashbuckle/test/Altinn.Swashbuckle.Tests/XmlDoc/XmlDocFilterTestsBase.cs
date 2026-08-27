using Altinn.Swashbuckle.XmlDoc;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocFilterTestsBase
{
    private readonly IXmlDocProvider _provider;

    // Use relaxed escaping so that " serializes as \" instead of ",
    // matching the output of the old IOpenApiAny.ToJson().
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public XmlDocFilterTestsBase()
    {
        _provider = new DefaultXmlDocProvider();
    }

    protected IXmlDocProvider Provider => _provider;

    protected static string ToJsonString(JsonNode? node) => node?.ToJsonString(_jsonOptions) ?? "null";
}
