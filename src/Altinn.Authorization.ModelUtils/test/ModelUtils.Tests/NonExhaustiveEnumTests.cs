using Altinn.Authorization.ModelUtils.Tests.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Tests;

public class NonExhaustiveEnumTests
{
    [Fact]
    public void Enum_ImplicitlyConverts()
    {
        var value = Enums.Default.OtherValue3;
        NonExhaustiveEnum<Enums.Default> nonExhaustive = value;

        nonExhaustive.ShouldBe(value);
        nonExhaustive.IsWellKnown.ShouldBeTrue();
        nonExhaustive.IsUnknown.ShouldBeFalse();
    }

    [Fact]
    public void WellKnownValue_ExplicitlyCastsToEnum_DoesNotThrow()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Enums.Default.OtherValue3;
        var value = Should.NotThrow(() => (Enums.Default)nonExhaustive);

        value.ShouldBe(Enums.Default.OtherValue3);
    }

    [Fact]
    public void WellKnownValue_ExplicitlyCastsToString_Throws()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Enums.Default.OtherValue3;
        Should.Throw<InvalidCastException>(() => (string)nonExhaustive);
    }

    [Fact]
    public void UnknownValue_ExplicitlyCastsToEnum_Throws()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Json.Deserialize<NonExhaustiveEnum<Enums.Default>>(@"""not-a-value""");
        Should.Throw<InvalidCastException>(() => (Enums.Default)nonExhaustive);
    }

    [Fact]
    public void UnknownValue_ExplicitlyCastsToString_DoesNotThrow()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Json.Deserialize<NonExhaustiveEnum<Enums.Default>>(@"""not-a-value""");
        var value = Should.NotThrow(() => (string)nonExhaustive);
        
        value.ShouldBe("not-a-value");
    }

    [Fact]
    public void WellKnownValue_Value_DoesNotThrow()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Enums.Default.OtherValue3;
        var value = Should.NotThrow(() => nonExhaustive.Value);

        value.ShouldBe(Enums.Default.OtherValue3);
    }

    [Fact]
    public void WellKnownValue_UnknownValue_Throws()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Enums.Default.OtherValue3;
        Should.Throw<InvalidCastException>(() => nonExhaustive.UnknownValue);
    }

    [Fact]
    public void UnknownValue_Value_Throws()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Json.Deserialize<NonExhaustiveEnum<Enums.Default>>(@"""not-a-value""");
        Should.Throw<InvalidCastException>(() => nonExhaustive.Value);
    }

    [Fact]
    public void UnknownValue_UnknownValue_DoesNotThrow()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Json.Deserialize<NonExhaustiveEnum<Enums.Default>>(@"""not-a-value""");
        var value = Should.NotThrow(() => nonExhaustive.UnknownValue);
        
        value.ShouldBe("not-a-value");
    }

    [Fact]
    public void UnknownValue_IsUnknown()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Json.Deserialize<NonExhaustiveEnum<Enums.Default>>(@"""not-a-value""");
        
        nonExhaustive.IsUnknown.ShouldBeTrue();
        nonExhaustive.IsWellKnown.ShouldBeFalse();
    }

    [Fact]
    public void WellKnownValue_IsWellKnown()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Enums.Default.OtherValue3;
        
        nonExhaustive.IsWellKnown.ShouldBeTrue();
        nonExhaustive.IsUnknown.ShouldBeFalse();
    }

    [Fact]
    public void WellKnown_Select_ReturnsWellKnown()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Enums.Default.OtherValue3;

        var mapped = nonExhaustive.Select(static _ => Enums.KebabCaseLower.OtherValue3);
        mapped.ShouldBe(Enums.KebabCaseLower.OtherValue3);
    }

    [Fact]
    public void Unknown_Select_ReturnsUnknown()
    {
        NonExhaustiveEnum<Enums.Default> nonExhaustive = Json.Deserialize<NonExhaustiveEnum<Enums.Default>>(@"""other-value-3""");

        var mapped = nonExhaustive.Select(static _ => Enums.KebabCaseLower.OtherValue3);
        mapped.IsUnknown.ShouldBeTrue();
        mapped.UnknownValue.ShouldBe("other-value-3");
    }

#pragma warning disable CS1718 // Comparison made to same variable
    [Fact]
    public void Equality()
    {
        NonExhaustiveEnum<Enums.Default> known3 = Enums.Default.OtherValue3;
        NonExhaustiveEnum<Enums.Default> unknown = Json.Deserialize<NonExhaustiveEnum<Enums.Default>>(@"""not-a-value""");

        known3.Equals(known3).ShouldBeTrue();
        known3.Equals(Enums.Default.OtherValue3).ShouldBeTrue();
        known3.Equals(Enums.Default.SomeValue1).ShouldBeFalse();
        known3.Equals(unknown).ShouldBeFalse();
        known3.Equals("not-a-value").ShouldBeFalse();

        unknown.Equals(known3).ShouldBeFalse();
        unknown.Equals(Enums.Default.OtherValue3).ShouldBeFalse();
        unknown.Equals(Enums.Default.SomeValue1).ShouldBeFalse();
        unknown.Equals(unknown).ShouldBeTrue();
        unknown.Equals("not-a-value").ShouldBeTrue();

        known3.Equals((object)known3).ShouldBeTrue();
        known3.Equals((object)Enums.Default.OtherValue3).ShouldBeTrue();
        known3.Equals((object)Enums.Default.SomeValue1).ShouldBeFalse();
        known3.Equals((object)unknown).ShouldBeFalse();
        known3.Equals((object)"not-a-value").ShouldBeFalse();

        unknown.Equals((object)known3).ShouldBeFalse();
        unknown.Equals((object)Enums.Default.OtherValue3).ShouldBeFalse();
        unknown.Equals((object)Enums.Default.SomeValue1).ShouldBeFalse();
        unknown.Equals((object)unknown).ShouldBeTrue();
        unknown.Equals((object)"not-a-value").ShouldBeTrue();

        (known3 == known3).ShouldBeTrue();
        (known3 == Enums.Default.OtherValue3).ShouldBeTrue();
        (known3 == Enums.Default.SomeValue1).ShouldBeFalse();
        (known3 == unknown).ShouldBeFalse();
        (known3 == "not-a-value").ShouldBeFalse();

        (unknown == known3).ShouldBeFalse();
        (unknown == Enums.Default.OtherValue3).ShouldBeFalse();
        (unknown == Enums.Default.SomeValue1).ShouldBeFalse();
        (unknown == unknown).ShouldBeTrue();
        (unknown == "not-a-value").ShouldBeTrue();

        (known3 != known3).ShouldBeFalse();
        (known3 != Enums.Default.OtherValue3).ShouldBeFalse();
        (known3 != Enums.Default.SomeValue1).ShouldBeTrue();
        (known3 != unknown).ShouldBeTrue();
        (known3 != "not-a-value").ShouldBeTrue();

        (unknown != known3).ShouldBeTrue();
        (unknown != Enums.Default.OtherValue3).ShouldBeTrue();
        (unknown != Enums.Default.SomeValue1).ShouldBeTrue();
        (unknown != unknown).ShouldBeFalse();
        (unknown != "not-a-value").ShouldBeFalse();
    }
#pragma warning restore CS1718 // Comparison made to same variable

    [Fact]
    public void DictionaryKeys()
    {
        NonExhaustiveEnum<Enums.Default> unknown = Json.Deserialize<NonExhaustiveEnum<Enums.Default>>(@"""not-a-value""");
        var json =
            """
            {
              "OtherValue3": 3,
              "not-a-value": 4
            }
            """;


        var dict = Json.Deserialize<Dictionary<NonExhaustiveEnum<Enums.Default>, int>>(json);
        dict.ShouldNotBeNull();
        dict.ShouldContainKeyAndValue(Enums.Default.OtherValue3, 3);
        dict.ShouldContainKeyAndValue(unknown, 4);
    }

#if DEBUG
    [Fact]
    public void ThrowsOnNumericEnumConverter()
    {
        var exn = Should.Throw<InvalidOperationException>(() => Json.Deserialize<NonExhaustiveEnum<TestEnum>>(@"1"));
        exn.Message.ShouldBe($"Enum value '{TestEnum.One}' of '{typeof(TestEnum)}' is not serialized as a string, did you forget to decorate it with {nameof(StringEnumConverterAttribute)}?");
    }
#endif

    [Theory]
    [Enums.NonExhaustiveEnumCasesData]
    public void JsonRoundTrip(Enums.NonExhaustiveEnumCaseModel model)
    {
        using var doc = Json.SerializeToDocument(model.Value, model.NonExhaustiveType);
        doc.RootElement.ValueKind.ShouldBe(JsonValueKind.String);
        doc.RootElement.GetString().ShouldBe(model.JsonValue);

        var deserialized = Json.Deserialize(doc, model.NonExhaustiveType);
        deserialized.ShouldBe(model.Value);
    }

    [JsonConverter(typeof(NumberEnumConverter))]
    private enum TestEnum
        : ulong
    {
        One = 1,
        Two = 2,
    }

    private sealed class NumberEnumConverter
        : JsonConverter<TestEnum>
    {
        public override TestEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (TestEnum)reader.GetUInt64();
        }

        public override void Write(Utf8JsonWriter writer, TestEnum value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((ulong)value);
        }
    }
}
