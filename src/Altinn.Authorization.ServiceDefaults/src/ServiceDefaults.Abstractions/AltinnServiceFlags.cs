namespace Altinn.Authorization.ServiceDefaults;

[Flags]
internal enum AltinnServiceFlags
    : byte
{
    None = default,
    RunInitOnly = 1 << 0,
}
