using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Defines a writer that can write formatted output to the console.
/// </summary>
public interface IFormatWriter
{
    /// <summary>
    /// Gets the application services associated with the console.
    /// </summary>
    IServiceProvider ApplicationServices { get; }

    /// <summary>
    /// Gets the console profile.
    /// </summary>
    Profile Profile { get; }

    /// <summary>
    /// Writes a <see cref="IRenderable"/> to the console.
    /// </summary>
    /// <param name="renderable">The <see cref="IRenderable"/> to write.</param>
    void Write(IRenderable renderable);

    /// <summary>
    /// Writes ANSI/VT escapes sequences to the console.
    /// </summary>
    /// <param name="action">The <see cref="AnsiWriter"/> action.</param>
    void WriteAnsi(Action<AnsiWriter> action);
}
