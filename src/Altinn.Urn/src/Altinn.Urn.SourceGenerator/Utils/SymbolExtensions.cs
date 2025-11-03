using Microsoft.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.Utils;

/// <summary>
/// Provides extension methods for working with Roslyn symbol types.
/// </summary>
public static class SymbolExtensions
{
    /// <summary>
    /// Determines whether the specified type symbol implements the given interface symbol.
    /// </summary>
    /// <remarks>This method checks for both direct and indirect interface implementations, including those
    /// inherited from base types. Generic type parameters are considered when determining implementation.</remarks>
    /// <param name="typeSymbol">The type symbol to examine for interface implementation. Cannot be <see langword="null"/>.</param>
    /// <param name="interfaceSymbol">The interface type symbol to check for. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified type symbol implements the given interface; otherwise, <see langword="false"/>.</returns>
    public static bool Implements(this ITypeSymbol typeSymbol, ITypeSymbol interfaceSymbol)
    {
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            if (iface.Equals(interfaceSymbol, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        return false;
    }
}
