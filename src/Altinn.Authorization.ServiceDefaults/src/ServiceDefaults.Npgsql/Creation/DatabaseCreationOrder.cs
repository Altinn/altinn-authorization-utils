using CommunityToolkit.Diagnostics;
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
    public static readonly DatabaseCreationOrder CreateDatabases = new(CREATE_DATABASES); // default
    private const int CREATE_DATABASES = 0;

    /// <summary>
    /// The "create roles" step - must run before database is created because a database
    /// can be owned by one of the created roles.
    /// </summary>
    public static readonly DatabaseCreationOrder CreateRoles = new(CREATE_ROLES); // before databases
    private const int CREATE_ROLES = -20;

    /// <summary>
    /// The "configure roles" step - must run before database is created because a database
    /// can be owned by one of the configured roles.
    /// </summary>
    public static readonly DatabaseCreationOrder ConfigureRoles = new(CONFIGURE_ROLES); // before databases
    private const int CONFIGURE_ROLES = -10;

    /// <summary>
    /// The "create schemas" step - must run after database is created because it can
    /// create schemas in the newly created database.
    /// </summary>
    public static readonly DatabaseCreationOrder CreateSchemas = new(CREATE_SCHEMAS); // after databases
    private const int CREATE_SCHEMAS = 10;

    /// <summary>
    /// The "create grants" step - must run after database and schemas is created because it can
    /// grant permissions on the newly created database/schemas.
    /// </summary>
    public static readonly DatabaseCreationOrder CreateGrants = new(CREATE_GRANTS); // after schemas
    private const int CREATE_GRANTS = 20;

    /// <summary>
    /// The "alter default privileges" step - must run after grants because it can
    /// require grants to be in place.
    /// </summary>
    public static readonly DatabaseCreationOrder AlterDefaultPrivileges = new(ALTER_DEFAULT_PRIVILEGES); // after grants
    private const int ALTER_DEFAULT_PRIVILEGES = 30;

    private readonly int _value;

    private DatabaseCreationOrder(int value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public override string ToString()
        => _value switch
        {
            CREATE_DATABASES => nameof(CreateDatabases),
            CREATE_ROLES => nameof(CreateRoles),
            CONFIGURE_ROLES => nameof(ConfigureRoles),
            CREATE_SCHEMAS => nameof(CreateSchemas),
            CREATE_GRANTS => nameof(CreateGrants),
            ALTER_DEFAULT_PRIVILEGES => nameof(AlterDefaultPrivileges),
            _ => throw new UnreachableException(),
        };

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

    private string DebuggerDisplay
        => ToString();
}
