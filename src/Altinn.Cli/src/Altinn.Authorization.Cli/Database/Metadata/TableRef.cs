using System.Diagnostics;

namespace Altinn.Authorization.Cli.Database.Metadata;

/// <summary>
/// Represents a reference to a table.
/// </summary>
[DebuggerDisplay("Table {Schema}.{Name}")]
public class TableRef(string schema, string name)
    : DbObjectRef(schema, name)
{
}
