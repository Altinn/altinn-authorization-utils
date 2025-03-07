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
