using CommunityToolkit.Diagnostics;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Represents a rich formatter that can format objects as rich text and write them to the console.
/// </summary>
public sealed class RichFormat
    : IFormat
{
    /// <inheritdoc/>
    public string Name => "rich";

    /// <inheritdoc/>
    public ValueTask Write<T>(T value, IFormatWriter writer, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(value);
        Guard.IsAssignableToType<IRenderable>(value);

        writer.Write((IRenderable)value);
        writer.WriteAnsi(w => w.WriteLine());
        return ValueTask.CompletedTask;
    }
}
