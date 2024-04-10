namespace Altinn.Urn.Visit;

/// <summary>
/// A visitor for key-value URNs.
/// </summary>
public interface IKeyValueUrnVisitor
{
    public void Visit<TUrn, TVariant, TValue>(TUrn urn, TVariant variant, TValue value) 
        where TUrn : IKeyValueUrn<TUrn, TVariant>
        where TVariant : struct, Enum;
}
