namespace Altinn.Urn;

/// <summary>
/// Extension methods for <see cref="IUrn"/>.
/// </summary>
public static class UrnExtensions 
{
    /// <summary>
    /// Get the URN as a <see cref="RawUrn"/>.
    /// </summary>
    /// <param name="urn">The URN.</param>
    /// <returns>A <see cref="RawUrn"/>, created from <paramref name="urn"/>.</returns>
    public static RawUrn AsRaw<TUrn>(this TUrn urn)
        where TUrn : IUrn
    {
        if (typeof(TUrn) == typeof(RawUrn))
        {
            return (RawUrn)(object)urn;
        }

        return RawUrn.Create(urn);
    }
}
