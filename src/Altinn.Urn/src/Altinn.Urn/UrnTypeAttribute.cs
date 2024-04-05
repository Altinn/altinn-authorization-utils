namespace Altinn.Urn;

/// <summary>
/// Marker attribute used by the source generator to generate URN types.
/// Applied to "type" methods to add valid URN prefixes.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class UrnTypeAttribute : Attribute
{
    public string Prefix { get; }

    public bool Canonical { get; set; }

    public UrnTypeAttribute(string prefix)
    {
        Prefix = prefix;
    }
}
