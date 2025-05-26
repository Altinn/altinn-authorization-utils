using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Models;

/// <summary>
/// Sample enums for JSON serialization.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Enums
{
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

    /// <summary>
    /// Test enum with default naming policy.
    /// </summary>
    [StringEnumConverter]
    public enum Default
    {
        /// <summary>
        /// Some value 1.
        /// </summary>
        SomeValue1,

        /// <summary>
        /// Second value 2.
        /// </summary>
        SecondValue2,

        /// <summary>
        /// Other value 3.
        /// </summary>
        OtherValue3,

#if NET9_0_OR_GREATER
        /// <summary>
        /// Custom value 4.
        /// </summary>
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    /// <summary>
    /// Test enum with camel case naming policy.
    /// </summary>
    [StringEnumConverter(JsonKnownNamingPolicy.CamelCase)]
    public enum CamelCase
    {
        /// <summary>
        /// Some value 1.
        /// </summary>
        SomeValue1,

        /// <summary>
        /// Second value 2.
        /// </summary>
        SecondValue2,

        /// <summary>
        /// Other value 3.
        /// </summary>
        OtherValue3,

#if NET9_0_OR_GREATER
        /// <summary>
        /// Custom value 4.
        /// </summary>
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    /// <summary>
    /// Test enum with pascal case naming policy.
    /// </summary>
    [StringEnumConverter(JsonKnownNamingPolicy.KebabCaseLower)]
    public enum KebabCaseLower
    {
        /// <summary>
        /// Some value 1.
        /// </summary>
        SomeValue1,

        /// <summary>
        /// Second value 2.
        /// </summary>
        SecondValue2,

        /// <summary>
        /// Other value 3.
        /// </summary>
        OtherValue3,

#if NET9_0_OR_GREATER
        /// <summary>
        /// Custom value 4.
        /// </summary>
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    /// <summary>
    /// Test enum with pascal case naming policy.
    /// </summary>
    [StringEnumConverter(JsonKnownNamingPolicy.KebabCaseUpper)]
    public enum KebabCaseUpper
    {
        /// <summary>
        /// Some value 1.
        /// </summary>
        SomeValue1,

        /// <summary>
        /// Second value 2.
        /// </summary>
        SecondValue2,

        /// <summary>
        /// Other value 3.
        /// </summary>
        OtherValue3,

#if NET9_0_OR_GREATER
        /// <summary>
        /// Custom value 4.
        /// </summary>
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    /// <summary>
    /// Test enum with snake case naming policy.
    /// </summary>
    [StringEnumConverter(JsonKnownNamingPolicy.SnakeCaseLower)]
    public enum SnakeCaseLower
    {
        /// <summary>
        /// Some value 1.
        /// </summary>
        SomeValue1,

        /// <summary>
        /// Second value 2.
        /// </summary>
        SecondValue2,

        /// <summary>
        /// Other value 3.
        /// </summary>
        OtherValue3,

#if NET9_0_OR_GREATER
        /// <summary>
        /// Custom value 4.
        /// </summary>
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    /// <summary>
    /// Test enum with snake case naming policy.
    /// </summary>
    [StringEnumConverter(JsonKnownNamingPolicy.SnakeCaseUpper)]
    public enum SnakeCaseUpper
    {
        /// <summary>
        /// Some value 1.
        /// </summary>
        SomeValue1,

        /// <summary>
        /// Second value 2.
        /// </summary>
        SecondValue2,

        /// <summary>
        /// Other value 3.
        /// </summary>
        OtherValue3,

#if NET9_0_OR_GREATER
        /// <summary>
        /// Custom value 4.
        /// </summary>
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }

    /// <summary>
    /// Test enum with lower case naming policy.
    /// </summary>
    [LowerCaseStringEnumConverter]
    public enum LowerCase
    {
        /// <summary>
        /// Some value 1.
        /// </summary>
        SomeValue1,

        /// <summary>
        /// Second value 2.
        /// </summary>
        SecondValue2,

        /// <summary>
        /// Other value 3.
        /// </summary>
        OtherValue3,

#if NET9_0_OR_GREATER
        /// <summary>
        /// Custom value 4.
        /// </summary>
        [JsonStringEnumMemberName("custom")]
        CustomValue4,
#endif
    }
}
