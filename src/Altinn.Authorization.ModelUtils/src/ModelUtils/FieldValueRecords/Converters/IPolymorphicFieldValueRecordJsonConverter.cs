using Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

/// <summary>
/// A <see cref="JsonConverter"/> for a polymorphic field-value-record.
/// </summary>
public interface IPolymorphicFieldValueRecordJsonConverter
{
    /// <summary>
    /// Gets the JSON-name of the discriminator property.
    /// </summary>
    public string DiscriminatorPropertyName { get; }

    /// <summary>
    /// Gets the model for the field-value-record.
    /// </summary>
    public IPolymorphicFieldValueRecordModel Model { get; }

    /// <summary>
    /// Attempts to locate a property model based on the provided name. Returns a boolean indicating success or failure.
    /// </summary>
    /// <param name="name">The name of the property being searched for.</param>
    /// <param name="model">Outputs the found property model if the search is successful.</param>
    /// <returns>Indicates whether the property model was found.</returns>
    public bool TryFindPropertyModel(string name, [NotNullWhen(true)] out IFieldValueRecordPropertyModel? model);

    /// <summary>
    /// Gets a value indicating whether the provided model is the discriminator property for this <see cref="Model"/>.
    /// </summary>
    /// <param name="model">The property model.</param>
    /// <returns>Indicates whether the property model was the discriminator property.</returns>
    public bool IsDiscriminatorProperty(IFieldValueRecordPropertyModel model);
}
