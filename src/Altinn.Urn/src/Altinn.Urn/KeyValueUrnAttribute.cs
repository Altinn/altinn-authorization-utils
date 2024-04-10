namespace Altinn.Urn;

/// <summary>
/// Marker attribute used by the source generator to generate a key-value URN types.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class KeyValueUrnAttribute : Attribute
{
}

