namespace Altinn.Urn;

/// <summary>
/// Marker attribute used by the source generator to generate URN keys.
/// Applied to "variant" methods to add valid URN keys.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class UrnKeyAttribute : Attribute
{
    public string Key { get; }

    public bool Canonical { get; set; }

    public UrnKeyAttribute(string key)
    {
        Key = key;
    }
}
