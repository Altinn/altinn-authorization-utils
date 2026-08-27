namespace Altinn.Authorization.CommandLine.Arguments;

/// <summary>
/// An attribute that indicates that an enum type has custom parsing behavior for command line arguments/options.
/// </summary>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public sealed class ArgumentEnumAttribute
    : Attribute;
