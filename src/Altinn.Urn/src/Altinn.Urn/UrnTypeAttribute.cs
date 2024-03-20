namespace Altinn.Urn;

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
