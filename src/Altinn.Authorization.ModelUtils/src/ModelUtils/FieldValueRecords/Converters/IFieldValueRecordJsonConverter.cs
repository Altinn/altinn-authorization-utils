using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

/// <summary>
/// A <see cref="JsonConverter"/> for a field-value-record.
/// </summary>
public interface IFieldValueRecordJsonConverter
{
    /// <summary>
    /// Gets the model for the field-value-record.
    /// </summary>
    public FieldValueRecordModel Model { get; }

    /// <summary>
    /// Attempts to locate a property model based on the provided name. Returns a boolean indicating success or failure.
    /// </summary>
    /// <param name="name">The name of the property being searched for.</param>
    /// <param name="model">Outputs the found property model if the search is successful.</param>
    /// <returns>Indicates whether the property model was found.</returns>
    public bool TryFindPropertyModel(string name, [NotNullWhen(true)] out IFieldValueRecordPropertyModel? model);
}
