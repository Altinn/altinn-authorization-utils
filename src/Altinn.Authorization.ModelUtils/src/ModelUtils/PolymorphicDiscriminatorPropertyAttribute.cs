namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Specifies that this property is the type discriminator for a polymorphic field-value-record.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class PolymorphicDiscriminatorPropertyAttribute
    : Attribute
{
}
