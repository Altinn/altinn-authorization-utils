using Altinn.Authorization.ModelUtils.Tests.Utils;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.Tests;

public class StringEnumConverterAttributeTests
{
    [Theory]
    [Enums.EnumCasesData]
    public void JsonRoundTrip(Enums.EnumCaseModel model)
    {
        using var doc = Json.SerializeToDocument(model.Value, model.EnumType);
        doc.RootElement.ValueKind.ShouldBe(JsonValueKind.String);
        doc.RootElement.GetString().ShouldBe(model.JsonValue);

        var deserialized = Json.Deserialize(doc, model.EnumType);
        deserialized.ShouldBe(model.Value);
    }
}
