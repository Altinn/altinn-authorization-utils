using Altinn.Authorization.ModelUtils.Tests.Utils;
using Altinn.Authorization.ModelUtils.Tests.Utils.Shouldly;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.Tests;

public class JsonExtensionDataTests
{
    [Fact]
    public void HasExtensionData_Unset_ReturnsFalse()
    {
        IHasExtensionData model = new WithExtensionData(default);
        
        model.HasJsonExtensionData.ShouldBeFalse();
    }

    [Fact]
    public void HasExtensionData_Object_ReturnsTrue()
    {
        IHasExtensionData model = new WithExtensionData(Json.Deserialize<JsonElement>("{}"));
        
        model.HasJsonExtensionData.ShouldBeTrue();
    }

    [Fact]
    public void JsonExtensionProperties_Unset_IsEmpty()
    {
        IHasExtensionData model = new WithExtensionData(default);
        
        model.JsonExtensionProperties.ShouldBeEmpty();
    }

    [Fact]
    public void JsonExtensionProperties_Object_ReturnsProperties()
    {
        IHasExtensionData model = new WithExtensionData(Json.Deserialize<JsonElement>(
            """
            { "property1": "value1", "property2": 42 }
            """));
        
        var props = model.JsonExtensionProperties.ToList();
        props.Count.ShouldBe(2);
        props[0].Name.ShouldBe("property1");
        props[0].Value.ShouldBeStructurallyEquivalentTo("\"value1\"");
        props[1].Name.ShouldBe("property2");
        props[1].Value.ShouldBeStructurallyEquivalentTo("42");
    }

    private class WithExtensionData(JsonElement extensionData)
        : IHasExtensionData
    {
        JsonElement IHasExtensionData.JsonExtensionData
            => extensionData;
    }
}
