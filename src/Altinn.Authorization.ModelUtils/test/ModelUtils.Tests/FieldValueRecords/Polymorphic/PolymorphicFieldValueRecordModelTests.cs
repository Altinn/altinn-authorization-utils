using Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;

namespace Altinn.Authorization.ModelUtils.Tests.FieldValueRecords.Polymorphic;

public class PolymorphicFieldValueRecordModelTests
{
    [Fact]
    public void NotChildType_ShouldThrowInvalidOperationException()
    {
        var exn = Should.Throw<InvalidOperationException>(PolymorphicFieldValueRecordModel.For<WrongHierarchyType>);
    }

    public enum WrongHierarchy
    {
        NotChild
    }

    [PolymorphicFieldValueRecord(IsRoot = true)]
    [PolymorphicDerivedType(typeof(NotChild), WrongHierarchy.NotChild)]
    public record WrongHierarchyType(WrongHierarchy type)
    {
        [PolymorphicDiscriminatorProperty]
        public WrongHierarchy Type => type;
    }

    public record NotChild { }
}
