using Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

/// <summary>
/// A <see cref="JsonConverter"/> for a polymorphic field-value-record.
/// </summary>
public interface IPolymorphicFieldValueRecordJsonConverter
{
    /// <summary>
    /// Gets the model for the field-value-record.
    /// </summary>
    public IPolymorphicFieldValueRecordModel Model { get; }
}
