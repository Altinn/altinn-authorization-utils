using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved for use by a compiler for tracking metadata.
/// This attribute should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class NullableContextAttribute : Attribute
{
    /// <summary>Flag specifying metadata related to nullable reference types.</summary>
    public readonly byte Flag;

    /// <summary>Initializes the attribute.</summary>
    /// <param name="value">The flag value.</param>
    public NullableContextAttribute(byte value)
    {
        Flag = value;
    }
}
