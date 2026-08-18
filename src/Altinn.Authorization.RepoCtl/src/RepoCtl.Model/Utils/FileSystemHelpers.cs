namespace Altinn.Authorization.RepoCtl.Model.Utils;

internal static class FileSystemHelpers
{
    extension(DirectoryInfo directory)
    {
        public bool IsDescendantOf(DirectoryInfo ancestor)
        {
            var current = directory;
            while (current is not null)
            {
                if (current.FullName == ancestor.FullName)
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
    }
}
