using Altinn.Authorization.ModelUtils.Tests.Utils;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Altinn.Authorization.ModelUtils.Tests.NonExhaustive_Class_Tests;
using static Altinn.Authorization.ModelUtils.Tests.NonExhaustive_Struct_Tests;

namespace Altinn.Authorization.ModelUtils.Tests;

public static class NonExhaustiveTests
{
    public interface ITestType<TSelf>
        where TSelf : notnull, ITestType<TSelf>, IEquatable<TSelf>
    {
        public static abstract TSelf Value1 { get; }
        public static abstract TSelf Value2 { get; }
    }
}

public abstract class NonExhaustiveTests<T>
    where T : NonExhaustiveTests.ITestType<T>, IEquatable<T>
{
    [Fact]
    public void IsNonExhaustiveType_ReturnsTrue()
    {
        NonExhaustive.IsNonExhaustiveType(typeof(T), out _).ShouldBeFalse();
        NonExhaustive.IsNonExhaustiveType(typeof(NonExhaustive<T>), out var innerType).ShouldBeTrue();
        innerType.ShouldBe(typeof(T));
    }

    [Fact]
    public void Value_ImplicitlyConverts()
    {
        var value = T.Value1;
        NonExhaustive<T> nonExhaustive = value;

        nonExhaustive.ShouldBe(value);
        nonExhaustive.IsWellKnown.ShouldBeTrue();
        nonExhaustive.IsUnknown.ShouldBeFalse();
    }

    [Fact]
    public void WellKnownValue_ExplicitlyCastsToInner_DoesNotThrow()
    {
        NonExhaustive<T> nonExhaustive = T.Value1;
        var value = Should.NotThrow(() => (T)nonExhaustive);

        value.ShouldBe(T.Value1);
    }

    [Fact]
    public void WellKnownValue_ExplicitlyCastsToString_Throws()
    {
        NonExhaustive<T> nonExhaustive = T.Value1;
        Should.Throw<InvalidCastException>(() => (string)nonExhaustive);
    }

    [Fact]
    public void UnknownValue_ExplicitlyCastsToEnum_Throws()
    {
        NonExhaustive<T> nonExhaustive = Json.Deserialize<NonExhaustive<T>>(@"""not-a-value""")!;
        Should.Throw<InvalidCastException>(() => (T)nonExhaustive);
    }

    [Fact]
    public void UnknownValue_ExplicitlyCastsToString_DoesNotThrow()
    {
        NonExhaustive<T> nonExhaustive = Json.Deserialize<NonExhaustive<T>>(@"""not-a-value""")!;
        var value = Should.NotThrow(() => (string)nonExhaustive);

        value.ShouldBe("not-a-value");
    }

    [Fact]
    public void WellKnownValue_Value_DoesNotThrow()
    {
        NonExhaustive<T> nonExhaustive = T.Value1;
        var value = Should.NotThrow(() => nonExhaustive.Value);

        value.ShouldBe(T.Value1);
    }

    [Fact]
    public void WellKnownValue_UnknownValue_Throws()
    {
        NonExhaustive<T> nonExhaustive = T.Value1;
        Should.Throw<InvalidCastException>(() => nonExhaustive.UnknownValue);
    }

    [Fact]
    public void UnknownValue_Value_Throws()
    {
        NonExhaustive<T> nonExhaustive = Json.Deserialize<NonExhaustive<T>>(@"""not-a-value""")!;
        Should.Throw<InvalidCastException>(() => nonExhaustive.Value);
    }

    [Fact]
    public void UnknownValue_UnknownValue_DoesNotThrow()
    {
        NonExhaustive<T> nonExhaustive = Json.Deserialize<NonExhaustive<T>>(@"""not-a-value""")!;
        var value = Should.NotThrow(() => nonExhaustive.UnknownValue);

        value.ShouldBe("not-a-value");
    }

    [Fact]
    public void UnknownValue_IsUnknown()
    {
        NonExhaustive<T> nonExhaustive = Json.Deserialize<NonExhaustive<T>>(@"""not-a-value""")!;

        nonExhaustive.IsUnknown.ShouldBeTrue();
        nonExhaustive.IsWellKnown.ShouldBeFalse();
    }

    [Fact]
    public void WellKnownValue_IsWellKnown()
    {
        NonExhaustive<T> nonExhaustive = T.Value1;

        nonExhaustive.IsWellKnown.ShouldBeTrue();
        nonExhaustive.IsUnknown.ShouldBeFalse();
    }

#pragma warning disable CS1718 // Comparison made to same variable
    [Fact]
    public void Equality()
    {
        NonExhaustive<T> known1 = T.Value1;
        NonExhaustive<T> unknown = Json.Deserialize<NonExhaustive<T>>(@"""not-a-value""")!;

        known1.Equals(known1).ShouldBeTrue();
        known1.Equals(T.Value1).ShouldBeTrue();
        known1.Equals(T.Value2).ShouldBeFalse();
        known1.Equals(unknown).ShouldBeFalse();
        known1.Equals("not-a-value").ShouldBeFalse();

        unknown.Equals(known1).ShouldBeFalse();
        unknown.Equals(T.Value1).ShouldBeFalse();
        unknown.Equals(T.Value2).ShouldBeFalse();
        unknown.Equals(unknown).ShouldBeTrue();
        unknown.Equals("not-a-value").ShouldBeTrue();

        known1.Equals((object)known1).ShouldBeTrue();
        known1.Equals((object)T.Value1).ShouldBeTrue();
        known1.Equals((object)T.Value2).ShouldBeFalse();
        known1.Equals((object)unknown).ShouldBeFalse();
        known1.Equals((object)"not-a-value").ShouldBeFalse();

        unknown.Equals((object)known1).ShouldBeFalse();
        unknown.Equals((object)T.Value1).ShouldBeFalse();
        unknown.Equals((object)T.Value2).ShouldBeFalse();
        unknown.Equals((object)unknown).ShouldBeTrue();
        unknown.Equals((object)"not-a-value").ShouldBeTrue();

        (known1 == known1).ShouldBeTrue();
        (known1 == T.Value1).ShouldBeTrue();
        (known1 == T.Value2).ShouldBeFalse();
        (known1 == unknown).ShouldBeFalse();
        (known1 == "not-a-value").ShouldBeFalse();

        (unknown == known1).ShouldBeFalse();
        (unknown == T.Value1).ShouldBeFalse();
        (unknown == T.Value2).ShouldBeFalse();
        (unknown == unknown).ShouldBeTrue();
        (unknown == "not-a-value").ShouldBeTrue();

        (known1 != known1).ShouldBeFalse();
        (known1 != T.Value1).ShouldBeFalse();
        (known1 != T.Value2).ShouldBeTrue();
        (known1 != unknown).ShouldBeTrue();
        (known1 != "not-a-value").ShouldBeTrue();

        (unknown != known1).ShouldBeTrue();
        (unknown != T.Value1).ShouldBeTrue();
        (unknown != T.Value2).ShouldBeTrue();
        (unknown != unknown).ShouldBeFalse();
        (unknown != "not-a-value").ShouldBeFalse();
    }
#pragma warning restore CS1718 // Comparison made to same variable

    [Fact]
    public void DictionaryKeys()
    {
        NonExhaustive<T> unknown = Json.Deserialize<NonExhaustive<T>>(@"""not-a-value""")!;
        var json =
            """
            {
              "foo": 3,
              "not-a-value": 4
            }
            """;


        var dict = Json.Deserialize<Dictionary<NonExhaustive<T>, int>>(json);
        dict.ShouldNotBeNull();
        dict.ShouldContainKeyAndValue(T.Value1, 3);
        dict.ShouldContainKeyAndValue(unknown, 4);
    }
}

public class NonExhaustive_Struct_Tests
    : NonExhaustiveTests<Foo>
{
    [JsonConverter(typeof(JsonConverter))]
    public readonly record struct Foo
        : NonExhaustiveTests.ITestType<Foo>
    {
        static Foo NonExhaustiveTests.ITestType<Foo>.Value1 => new(1);
        static Foo NonExhaustiveTests.ITestType<Foo>.Value2 => new(2);

        private readonly byte _value;

        private Foo(byte value)
        {
            _value = value;
        }

        public sealed class JsonConverter
            : JsonConverter<Foo>
        {
            public override bool HandleNull => true;

            public override Foo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var value = reader.GetString();

                return value switch
                {
                    "foo" => new Foo(1),
                    "bar" => new Foo(2),
                    _ => throw new JsonException($"Unknown value: {value}"),
                };
            }

            public override Foo ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => Read(ref reader, typeToConvert, options);

            public override void Write(Utf8JsonWriter writer, Foo value, JsonSerializerOptions options)
            {
                switch (value._value)
                {
                    case 1:
                        writer.WriteStringValue("foo");
                        break;

                    case 2:
                        writer.WriteStringValue("bar");
                        break;

                    default:
                        throw new UnreachableException();
                }
            }

            public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] Foo value, JsonSerializerOptions options)
            {
                switch (value._value)
                {
                    case 1:
                        writer.WritePropertyName("foo");
                        break;

                    case 2:
                        writer.WritePropertyName("bar");
                        break;

                    default:
                        throw new UnreachableException();
                }
            }
        }
    }
}


public class NonExhaustive_Class_Tests
    : NonExhaustiveTests<Bar>
{
    [JsonConverter(typeof(JsonConverter))]
    public sealed record class Bar
        : NonExhaustiveTests.ITestType<Bar>
    {
        static Bar NonExhaustiveTests.ITestType<Bar>.Value1 => Instance1;
        static Bar NonExhaustiveTests.ITestType<Bar>.Value2 => Instance2;

        public static Bar Instance1 { get; } = new Bar(1);
        public static Bar Instance2 { get; } = new Bar(2);

        private readonly byte _value;

        private Bar(byte value)
        {
            _value = value;
        }

        public sealed class JsonConverter
            : JsonConverter<Bar>
        {
            public override Bar Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var value = reader.GetString();

                return value switch
                {
                    "foo" => Instance1,
                    "bar" => Instance2,
                    _ => throw new JsonException($"Unknown value: {value}"),
                };
            }

            public override Bar ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => Read(ref reader, typeToConvert, options);

            public override void Write(Utf8JsonWriter writer, Bar value, JsonSerializerOptions options)
            {
                switch (value._value)
                {
                    case 1:
                        writer.WriteStringValue("foo");
                        break;

                    case 2:
                        writer.WriteStringValue("bar");
                        break;

                    default:
                        throw new UnreachableException();
                }
            }

            public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] Bar value, JsonSerializerOptions options)
            {
                switch (value._value)
                {
                    case 1:
                        writer.WritePropertyName("foo");
                        break;

                    case 2:
                        writer.WritePropertyName("bar");
                        break;

                    default:
                        throw new UnreachableException();
                }
            }
        }
    }
}
