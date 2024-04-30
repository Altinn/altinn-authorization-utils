using System.Diagnostics;
using System.Numerics;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Creation;

/// <summary>
/// A value that represents the order in which a database-create action should be executed.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly record struct DatabaseCreationOrder
    : IEquatable<DatabaseCreationOrder>
    , IComparable<DatabaseCreationOrder>
    , IEqualityOperators<DatabaseCreationOrder, DatabaseCreationOrder, bool>
    , IComparisonOperators<DatabaseCreationOrder, DatabaseCreationOrder, bool>
{
    /// <summary>
    /// Default value - the "create database" step.
    /// </summary>
    public static readonly DatabaseCreationOrder CreateDatabases = new(0); // default

    /// <summary>
    /// The "create roles" step - must run before database is created because a database
    /// can be owned by one of the created roles.
    /// </summary>
    public static readonly DatabaseCreationOrder CreateRoles = new(-10); // before databases

    /// <summary>
    /// The "create grants" step - must run after database is created because it can
    /// grant permissions on the newly created database.
    /// </summary>
    public static readonly DatabaseCreationOrder CreateGrants = new(10); // after databases

    private readonly int _value;

    private DatabaseCreationOrder(int value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public int CompareTo(DatabaseCreationOrder other)
        => _value.CompareTo(other._value);

    /// <inheritdoc/>
    public bool Equals(DatabaseCreationOrder other)
        => _value == other._value;

    /// <inheritdoc/>
    public override int GetHashCode()
        => _value.GetHashCode();

    /// <inheritdoc/>
    public static bool operator <(DatabaseCreationOrder left, DatabaseCreationOrder right)
        => left.CompareTo(right) < 0;

    /// <inheritdoc/>
    public static bool operator >(DatabaseCreationOrder left, DatabaseCreationOrder right)
        => left.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static bool operator <=(DatabaseCreationOrder left, DatabaseCreationOrder right)
        => left.CompareTo(right) <= 0;

    /// <inheritdoc/>
    public static bool operator >=(DatabaseCreationOrder left, DatabaseCreationOrder right)
        => left.CompareTo(right) >= 0;
}
