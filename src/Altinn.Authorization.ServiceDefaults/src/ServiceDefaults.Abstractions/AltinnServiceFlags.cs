namespace Altinn.Authorization.ServiceDefaults;

[Flags]
internal enum AltinnServiceFlags
    : byte
{
    None = default,
    /// <summary>
    /// Application is being run in init-only mode.
    /// </summary>
    RunInitOnly = 1 << 0,

    /// <summary>
    /// Application is running under test conditions (e.g. unit tests, integration tests, etc.).
    /// </summary>
    IsTest = 1 << 1,
}
