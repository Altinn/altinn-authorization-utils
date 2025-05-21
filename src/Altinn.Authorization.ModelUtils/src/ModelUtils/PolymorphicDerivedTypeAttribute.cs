using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// When placed on a type declaration, indicates that the specified subtype should be opted into polymorphic serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class PolymorphicDerivedTypeAttribute
    : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PolymorphicDerivedTypeAttribute"/> class.
    /// </summary>
    /// <param name="type">A derived type that should be supported in polymorphic serialization of the declared based type.</param>
    /// <param name="typeDiscriminator">The type discriminator identifier to be used for the serialization of the subtype.</param>
    public PolymorphicDerivedTypeAttribute(Type type, object typeDiscriminator)
    {
        Guard.IsNotNull(type);
        Guard.IsNotNull(typeDiscriminator);
        EnsureValidEnum(typeDiscriminator);

        Type = type;
        TypeDiscriminator = typeDiscriminator;
    }

    /// <summary>
    /// Gets the derived type that should be supported in polymorphic serialization of the declared base type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the type discriminator identifier to be used for the serialization of the subtype.
    /// </summary>
    public object TypeDiscriminator { get; }

    private static void EnsureValidEnum(object value, [CallerArgumentExpression("value")] string name = "")
    {
        var type = value.GetType();
        if (!type.IsEnum)
        {
            ThrowHelper.ThrowArgumentException(name, "Value must be an enum.");
        }

        var isDefined = Enum.IsDefined(type, value);
        if (!isDefined)
        {
            ThrowHelper.ThrowArgumentException(name, "Value must be a defined enum value.");
        }
    }
}
