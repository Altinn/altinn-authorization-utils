namespace Altinn.Urn.Visit;

/// <summary>
/// A visitor for key-value URNs.
/// </summary>
public interface IKeyValueUrnVisitor
{
    /// <summary>
    /// Visits a key-value URN.
    /// </summary>
    /// <typeparam name="TUrn">The URN type.</typeparam>
    /// <typeparam name="TVariants">The variants enum type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="urn">The URN.</param>
    /// <param name="variant">The URN variant.</param>
    /// <param name="value">The URN value.</param>
    public void Visit<TUrn, TVariants, TValue>(TUrn urn, TVariants variant, TValue value) 
        where TUrn : IKeyValueUrn<TUrn, TVariants>
        where TVariants : struct, Enum;
}
