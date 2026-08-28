namespace Altinn.Authorization.RepoCtl.Model.Utils;

/// <summary>
/// Extensions methods for file system operations.
/// </summary>
public static class FileSystemHelpers
{
    extension(DirectoryInfo directory)
    {
        /// <summary>
        /// Determines whether the current directory is a descendant of the specified ancestor directory.
        /// </summary>
        /// <param name="ancestor">The potential ancestor directory.</param>
        /// <returns><c>true</c> if the current directory is a descendant of the specified ancestor directory; otherwise, <c>false</c>.</returns>
        public bool IsDescendantOf(DirectoryInfo ancestor)
        {
            var ancestorFullName = Path.TrimEndingDirectorySeparator(ancestor.FullName);
            var current = directory;
            while (current is not null)
            {
                if (Path.TrimEndingDirectorySeparator(current.FullName) == ancestorFullName)
                {
                    return true;
                }

                if (current.Parent is { FullName: { } fn } && fn.Length > current.FullName.Length)
                {
                    // This is a safety check to prevent infinite loops in case of a malformed directory structure.
                    break;
                }

                current = current.Parent;
            }

            return false;
        }

        /// <summary>
        /// Finds a file with the specified names and extensions in the current directory.
        /// </summary>
        /// <param name="names">The potential file names to search for.</param>
        /// <param name="extensions">The potential file extensions to search for.</param>
        /// <param name="fileAccess">The file access mode to use when opening the file.</param>
        /// <returns>An <see cref="OpenedFileStream"/> if a matching file is found; otherwise, <see langword="null"/>.</returns>
        public OpenedFileStream? Find(
            ReadOnlySpan<string> names,
            ReadOnlySpan<string> extensions,
            FileAccess fileAccess = FileAccess.Read)
        {
            foreach (var name in names)
            {
                foreach (var extension in extensions)
                {
                    var file = new FileInfo(Path.Combine(directory.FullName, $"{name}.{extension}"));
                    try
                    {
                        var fs = file.Open(FileMode.Open, fileAccess);
                        return new(file, fs);
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a file with the specified names and extensions in the current directory or any of its ancestor directories.
        /// </summary>
        /// <param name="names">The potential file names to search for.</param>
        /// <param name="extensions">The potential file extensions to search for.</param>
        /// <param name="fileAccess">The file access mode to use when opening the file.</param>
        /// <returns>An <see cref="OpenedFileStream"/> if a matching file is found; otherwise, <see langword="null"/>.</returns>
        public OpenedFileStream? FindUp(
            ReadOnlySpan<string> names,
            ReadOnlySpan<string> extensions,
            FileAccess fileAccess = FileAccess.Read)
        {
            for (var current = directory; current is not null; current = current.Parent)
            {
                var result = current.Find(names, extensions, fileAccess);
                if (result is not null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
