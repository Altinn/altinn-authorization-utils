namespace Altinn.Urn.Visit;

/// <summary>
/// A visitable key-value URN.
/// </summary>
public interface IVisitableKeyValueUrn
{
    /// <summary>
    /// Accepts a visitor.
    /// </summary>
    /// <param name="visitor">A <see cref="IKeyValueUrnVisitor"/>.</param>
    public void Accept(IKeyValueUrnVisitor visitor);
}
