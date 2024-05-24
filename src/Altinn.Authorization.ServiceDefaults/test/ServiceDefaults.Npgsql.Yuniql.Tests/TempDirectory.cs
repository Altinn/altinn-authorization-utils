namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql.Tests;

public sealed class TempDirectory
    : IAsyncDisposable
{
    public static TempDirectory Create()
    {
        while (true)
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (Path.Exists(dir))
            {
                continue;
            }

            var info = Directory.CreateDirectory(dir);
            return new TempDirectory(info.FullName);
        }
    }

    private readonly string _dir;

    private TempDirectory(string dir) => _dir = dir;

    public ValueTask DisposeAsync()
    {
        var info = DirectoryInfo;
        if (info.Exists)
        {
            info.Delete(recursive: true);
        }

        return default;
    }

    public DirectoryInfo DirectoryInfo => new(_dir);
}
