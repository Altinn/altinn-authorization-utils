namespace Altinn.Urn.Visit;

/// <summary>
/// A visitable key-value URN.
/// </summary>
public interface IVisitableKeyValueUrn
{
    public void Accept(IKeyValueUrnVisitor visitor);
}
