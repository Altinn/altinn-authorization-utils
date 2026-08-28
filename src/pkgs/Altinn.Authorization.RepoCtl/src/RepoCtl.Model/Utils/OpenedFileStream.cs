namespace Altinn.Authorization.RepoCtl.Model.Utils;

/// <summary>
/// Represents a file stream that has been opened and keeps track of the associated file information.
/// </summary>
/// <param name="fileInfo">The file information associated with the opened file.</param>
/// <param name="stream">The underlying file stream.</param>
public sealed class OpenedFileStream(FileInfo fileInfo, FileStream stream)
    : DelegatingStream(stream)
{
    /// <summary>
    /// Gets the <see cref="FileInfo"/> associated with the opened file.
    /// </summary>
    public FileInfo FileInfo { get; } = fileInfo;
}
