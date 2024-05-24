namespace Altinn.Urn;

/// <summary>
/// Marker attribute used by the source generator to generate URN keys.
/// Applied to "variant" methods to add valid URN keys.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class UrnKeyAttribute : Attribute
{
    /// <summary>
    /// Gets the URN key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the URN key is the canonical representation for this variant.
    /// </summary>
    public bool Canonical { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrnKeyAttribute"/> class.
    /// </summary>
    /// <param name="key">The URN key.</param>
    public UrnKeyAttribute(string key)
    {
        Key = key;
    }
}
