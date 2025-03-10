using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Sdk;
using Xunit.v3;

namespace Altinn.Authorization.ModelUtils.Tests.Utils;

public static class Enums
{
    public static TheoryData<EnumModel> EnumTypes
        => [
            new EnumModel(typeof(Default)),
            new EnumModel(typeof(CamelCase)),
            new EnumModel(typeof(KebabCaseLower)),
            new EnumModel(typeof(KebabCaseUpper)),
            new EnumModel(typeof(SnakeCaseLower)),
            new EnumModel(typeof(SnakeCaseUpper)),
            new EnumModel(typeof(LowerCase)),
        ];

    public static TheoryData<EnumCaseModel> EnumCases
        => [
            .. EnumTypes.SelectMany(static model => model.Data.Cases),
        ];

    public static TheoryData<NonExhaustiveEnumCaseModel> NonExhaustiveEnumCases
        => [
            .. EnumTypes.SelectMany(static model => model.Data.Cases.Select(NonExhaustiveEnumCaseModel.From).Concat([new NonExhaustiveEnumCaseModel(model.Data.Type, "does-not-exist", "does-not-exist")]))
        ];

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class EnumCasesDataAttribute
        : DataAttribute
    {
        public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
            => new(EnumCases);

        public override bool SupportsDiscoveryEnumeration()
            => true;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class NonExhaustiveEnumCasesDataAttribute
        : DataAttribute
    {
        public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
            => new(NonExhaustiveEnumCases);

        public override bool SupportsDiscoveryEnumeration()
            => true;
    }

    private static string GetExpected(object value, Type type, JsonNamingPolicy? namingPolicy)
    {
        var name = value.ToString().ShouldNotBeNull();
#if NET9_0_OR_GREATER
        if (type.GetField(name, BindingFlags.Public | BindingFlags.Static)?.GetCustomAttribute<JsonStringEnumMemberNameAttribute>() is { } nameAttr)
        {
            return nameAttr.Name.ShouldNotBeNull();
        }
#endif

        return namingPolicy?.ConvertName(name) ?? name;
    }

    private static string GetExpected(object value, Type type)
    {
        var namingPolicy = type.GetCustomAttribute<StringEnumConverterAttribute>().ShouldNotBeNull().NamingPolicy;
        return GetExpected(value, type, namingPolicy);
    }

    public sealed class EnumModel
        : IXunitSerializable
    {
        private Type _type;
        private ImmutableArray<EnumCaseModel> _cases;

        // Required for deserialization
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public EnumModel()
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public EnumModel(Type type)
        {
            _type = type;
            _cases = GetCases(type);
        }

        public Type Type
            => _type;

        public ImmutableArray<EnumCaseModel> Cases
            => _cases;

        public void Deserialize(IXunitSerializationInfo info)
        {
            _type = info.GetValue<Type>("type")!;
            _cases = GetCases(_type);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("type", _type, typeof(Type));
        }

        private static ImmutableArray<EnumCaseModel> GetCases(Type type)
        {
            var values = Enum.GetValues(type);
            var namingPolicy = type.GetCustomAttribute<StringEnumConverterAttribute>().ShouldNotBeNull().NamingPolicy;

            var builder = ImmutableArray.CreateBuilder<EnumCaseModel>(values.Length);
            foreach (var value in values)
            {
                var expected = GetExpected(value, type, namingPolicy);
                builder.Add(new EnumCaseModel(type, value, expected));
            }

            return builder.MoveToImmutable();
        }
    }

    public sealed class EnumCaseModel
        : IXunitSerializable
    {
        private Type _enumType;
        private object _value;
        private string _expected;

        // Required for deserialization
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public EnumCaseModel()
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public EnumCaseModel(Type enumType, object value, string expected)
        {
            _enumType = enumType;
            _value = value;
            _expected = expected;
        }

        public Type EnumType
            => _enumType;

        public object Value
            => _value;

        public string JsonValue
            => _expected;

        public void Deserialize(IXunitSerializationInfo info)
        {
            _value = info.GetValue("value")!;
            _enumType = _value.GetType();
            _expected = GetExpected(_value, _enumType);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("value", _value, _enumType);
        }
    }

    public sealed class NonExhaustiveEnumCaseModel
        : IXunitSerializable
    {
        private Type _enumType;
        private Type _nonExhaustiveType;
        private object _value;
        private string _expected;

        internal static NonExhaustiveEnumCaseModel From(EnumCaseModel model)
            => new(model.EnumType, model.Value, model.JsonValue);

        // Required for deserialization
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public NonExhaustiveEnumCaseModel()
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public NonExhaustiveEnumCaseModel(Type enumType, object value, string expected)
        {
            _enumType = enumType;
            _nonExhaustiveType = typeof(NonExhaustiveEnum<>).MakeGenericType(enumType);
            _value = value;
            _expected = expected;
        }

        public Type EnumType
            => _enumType;

        public Type NonExhaustiveType
            => _nonExhaustiveType;

        public object Value
            => Activator.CreateInstance(_nonExhaustiveType, BindingFlags.NonPublic | BindingFlags.Instance, binder: null, args: [_value], culture: null)!;

        public string JsonValue
            => _expected;

        public void Deserialize(IXunitSerializationInfo info)
        {
            _enumType = info.GetValue<Type>("type")!;
            _nonExhaustiveType = typeof(NonExhaustiveEnum<>).MakeGenericType(_enumType);
            _value = info.GetValue("value")!;

            if (_value is string expected)
            {
                _expected = expected;
            }
            else
            {
                _expected = GetExpected(_value, _enumType);
            }
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("type", _enumType, typeof(Type));
            if (_value is string)
            {
                info.AddValue("value", _value, typeof(string));
            }
            else
            {
                info.AddValue("value", _value, _enumType);
            }
        }
    }

    private class LowerCaseNamingPolicy
        : JsonNamingPolicy
    {
        public override string ConvertName(string name)
            => name.ToLowerInvariant();
    }

    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    private class LowerCaseStringEnumConverterAttribute()
        : StringEnumConverterAttribute(new LowerCaseNamingPolicy())
    {
    }

    [StringEnumConverter]
    public enum Default
    {
        SomeValue1,
        SecondValue2,
        OtherValue3,
#if NET9_0_OR_GREATER
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    [StringEnumConverter(JsonKnownNamingPolicy.CamelCase)]
    public enum CamelCase
    {
        SomeValue1,
        SecondValue2,
        OtherValue3,
#if NET9_0_OR_GREATER
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    [StringEnumConverter(JsonKnownNamingPolicy.KebabCaseLower)]
    public enum KebabCaseLower
    {
        SomeValue1,
        SecondValue2,
        OtherValue3,
#if NET9_0_OR_GREATER
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    [StringEnumConverter(JsonKnownNamingPolicy.KebabCaseUpper)]
    public enum KebabCaseUpper
    {
        SomeValue1,
        SecondValue2,
        OtherValue3,
#if NET9_0_OR_GREATER
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    [StringEnumConverter(JsonKnownNamingPolicy.SnakeCaseLower)]
    public enum SnakeCaseLower
    {
        SomeValue1,
        SecondValue2,
        OtherValue3,
#if NET9_0_OR_GREATER
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    [StringEnumConverter(JsonKnownNamingPolicy.SnakeCaseUpper)]
    public enum SnakeCaseUpper
    {
        SomeValue1,
        SecondValue2,
        OtherValue3,
#if NET9_0_OR_GREATER
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    [LowerCaseStringEnumConverter]
    public enum LowerCase
    {
        SomeValue1,
        SecondValue2,
        OtherValue3,
#if NET9_0_OR_GREATER
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }
}
