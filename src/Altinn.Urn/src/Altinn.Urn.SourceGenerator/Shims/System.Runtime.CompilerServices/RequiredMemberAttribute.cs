using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>Specifies that a type has required members or that a member is required.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class RequiredMemberAttribute : Attribute
{
}
