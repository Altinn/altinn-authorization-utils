using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Defines a resolver that can resolve a formatter for a specific type.
/// </summary>
/// <typeparam name="TFormat">The type of the formatter.</typeparam>
public interface IFormatterResolver<TFormat>
    where TFormat : IFormat
{
    /// <summary>
    /// Tries to resolve a formatter for the specified type.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <param name="formatter">The resolved formatter if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a formatter was successfully resolved; otherwise, <see langword="false"/>.</returns>
    public bool TryResolve(Type type, [NotNullWhen(true)] out IFormatter<TFormat>? formatter);
}
