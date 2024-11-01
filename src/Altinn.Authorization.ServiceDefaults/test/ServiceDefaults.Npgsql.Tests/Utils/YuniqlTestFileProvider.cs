using Microsoft.Extensions.FileProviders;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests.Utils;

internal static class YuniqlTestFileProvider
{
    public static IFileProvider Create(IEnumerable<string> scripts)
    {
        var fs = new InMemoryFileProvider();

        var dir = fs.Root;
        var version = 0;
        foreach (var script in scripts)
        {
            var versionDir = dir.CreateSubdirectory($"v{version++}.00");
            versionDir.CreateFile("00-script.sql", script);
        }

        return fs;
    }
}
