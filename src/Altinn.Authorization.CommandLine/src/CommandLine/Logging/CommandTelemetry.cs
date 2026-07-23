using System.Diagnostics;

namespace Altinn.Authorization.CommandLine.Logging;

internal static class CommandTelemetry
{
    internal static readonly ActivitySource Source = new("Altinn.Authorization.Cli");
}
