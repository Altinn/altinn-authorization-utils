using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Models;

/// <summary>
/// Sample field-value-records for serialization.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FieldValueRecords
{
    /// <summary>
    /// An outer type containing an inner field-value.
    /// </summary>
    [FieldValueRecord]
    public record Outer
    {
        /// <summary>
        /// Gets the inner value.
        /// </summary>
        public required FieldValue<Inner> Inner { get; init; }
    }

    /// <summary>
    /// An inner type containing a field-value.
    /// </summary>
    [FieldValueRecord]
    public record Inner
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public required FieldValue<string> Value { get; init; }
    }

    [FieldValueRecord]
    public record RequiredOptional
    {
        public required string? RequiredNullable { get; init; }

        public required string RequiredNonNullable { get; init; }

        public string? OptionalNullable { get; init; }

        public required FieldValue<string> FieldValue { get; init; }
    }
}
