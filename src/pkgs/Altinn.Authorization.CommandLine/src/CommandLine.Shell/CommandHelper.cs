using System.Runtime.Versioning;

namespace Altinn.Authorization.CommandLine.Shell;

internal sealed class CommandHelper
{
    public static string Which(string command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        return OperatingSystem.IsWindows()
            ? WindowsWhich(command)
            : UnixWhich(command);
    }

    [SupportedOSPlatform("windows")]
    private static string WindowsWhich(string command)
    {
        if (Path.HasExtension(command))
        {
            return FindCommand(command, [string.Empty], requireExecutable: false);
        }

        var pathExtensions = Environment.GetEnvironmentVariable("PATHEXT");
        if (string.IsNullOrWhiteSpace(pathExtensions))
        {
            pathExtensions = ".COM;.EXE;.BAT;.CMD";
        }

        return FindCommand(
            command,
            pathExtensions
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(static extension => extension.StartsWith('.') ? extension : $".{extension}"),
            requireExecutable: false
        );
    }

    [UnsupportedOSPlatform("windows")]
    private static string UnixWhich(string command)
    {
        return FindCommand(command, [string.Empty], requireExecutable: true);
    }

    private static string FindCommand(
        string command,
        IEnumerable<string> extensions,
        bool requireExecutable
    )
    {
        var hasDirectory =
            command.Contains(Path.DirectorySeparatorChar)
            || command.Contains(Path.AltDirectorySeparatorChar);

        var directories = hasDirectory ? [string.Empty] : GetPathDirectories();
        foreach (var directory in directories)
        {
            foreach (var extension in extensions)
            {
                var candidate = string.IsNullOrEmpty(directory)
                    ? command + extension
                    : Path.Combine(directory, command + extension);

                if (File.Exists(candidate) && (!requireExecutable || IsExecutable(candidate)))
                {
                    return Path.GetFullPath(candidate);
                }
            }
        }

        throw new FileNotFoundException($"Command '{command}' was not found.", command);
    }

    private static IEnumerable<string> GetPathDirectories()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
        {
            yield break;
        }

        foreach (var directory in path.Split(Path.PathSeparator))
        {
            if (directory.Length == 0)
            {
                yield return Directory.GetCurrentDirectory();
            }
            else if (OperatingSystem.IsWindows())
            {
                yield return directory.Trim('"');
            }
            else
            {
                yield return directory;
            }
        }
    }

    private static bool IsExecutable(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            const UnixFileMode executable =
                UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;

            return (File.GetUnixFileMode(path) & executable) != 0;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
