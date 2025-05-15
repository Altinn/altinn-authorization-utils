using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Tests.FieldValueRecords.Polymorphic;

[StringEnumConverter(JsonKnownNamingPolicy.KebabCaseLower)]
public enum VariantType
{
    LeftChild,
    RightChild1,
    RightChild2,
    RightGrandChild,
}
