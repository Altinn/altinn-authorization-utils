using System.Text.Json.Serialization;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model.Checks;

/// <summary>
/// Represents a position in a text file, including line and optional column information.
/// </summary>
public readonly record struct LinePosition
{
    // note: 0 = unset for both line and column
    private readonly ushort _line;
    private readonly ushort _column;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinePosition"/> struct.
    /// </summary>
    /// <param name="line">The 1-indexed line number.</param>
    /// <param name="column">The 1-indexed column number.</param>
    /// <remarks>
    /// 0 is interpreted as unset for both line and column.
    /// If column is set, line is required.
    /// </remarks>
    public LinePosition(ushort line, ushort column)
    {
        if (column is not 0)
        {
            Guard.IsGreaterThan(line, (ushort)0);
        }

        _line = line;
        _column = column;
    }

    /// <summary>
    /// Gets a value indicating whether the current <see cref="LinePosition"/> has a value.
    /// </summary>
    [JsonIgnore]
    public bool HasValue
        => _line != 0;

    /// <summary>
    /// Gets a value indicating whether the current <see cref="LinePosition"/> has a column value.
    /// </summary>
    [JsonIgnore]
    public bool HasColumn
        => _column != 0;

    /// <summary>
    /// Gets the 1-indexed line number of the current <see cref="LinePosition"/>.
    /// </summary>
    public uint Line
        => _line;

    /// <summary>
    /// Gets the 1-indexed column number of the current <see cref="LinePosition"/>.
    /// </summary>
    public uint Column
        => _column;
}
