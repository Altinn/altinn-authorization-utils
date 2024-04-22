namespace Altinn.Urn.Visit;

/// <summary>
/// A visitor for key-value URNs.
/// </summary>
public interface IKeyValueUrnVisitor
{
    public void Visit<TUrn, TVariants, TValue>(TUrn urn, TVariants variant, TValue value) 
        where TUrn : IKeyValueUrn<TUrn, TVariants>
        where TVariants : struct, Enum;
}
