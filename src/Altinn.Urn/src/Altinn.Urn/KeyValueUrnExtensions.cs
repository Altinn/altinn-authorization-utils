namespace Altinn.Urn;

/// <summary>
/// Extension methods for <see cref="IKeyValueUrn"/>.
/// </summary>
public static class KeyValueUrnExtensions 
{
    /// <summary>
    /// Get the URN as a <see cref="KeyValueUrn"/>.
    /// </summary>
    /// <param name="urn">The URN.</param>
    /// <returns>A <see cref="KeyValueUrn"/>, created from <paramref name="urn"/>.</returns>
    public static KeyValueUrn AsKeyValueUrn<TUrn>(this TUrn urn)
        where TUrn : IKeyValueUrn
    {
        if (typeof(TUrn) == typeof(KeyValueUrn))
        {
            return (KeyValueUrn)(object)urn;
        }

        return KeyValueUrn.Create(urn);
    }
}
