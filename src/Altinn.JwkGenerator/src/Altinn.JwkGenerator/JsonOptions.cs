using System.Text.Json;

namespace Altinn.Authorization.JwkGenerator;

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
#if DEBUG
        WriteIndented = true,
#endif
    };
}
