using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
#if DEBUG
        WriteIndented = true,
#endif
    };
}
