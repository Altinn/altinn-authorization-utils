using System.Text;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Checks;

/// <summary>
/// Checks that no Bruno file (*.bru) starts with a UTF-8 byte order mark.
/// </summary>
internal sealed class ByteOrderMarkCheck
    : IRepositoryCheck
{
    /// <inheritdoc/>
    public string CheckId
        => "no-bom";

    /// <inheritdoc/>
    public string CheckDisplayName
        => "Files Without Byte Order Mark";

    /// <inheritdoc/>
    public IAsyncEnumerable<Diagnostic> Check(AltinnRepository repository, CancellationToken cancellationToken)
        => Check(repository.RootDirectory).ToAsyncEnumerable();

    internal static IEnumerable<Diagnostic> Check(DirectoryInfo rootDirectory)
        => new DirectoryInfo(Path.Combine(rootDirectory.FullName, "src"))
            .EnumerateFiles("*.bru", SearchOption.AllDirectories)
            .Where(StartsWithByteOrderMark)
            .Select(static file => Diagnostics.BrunoFileStartsWithByteOrderMark(file.FullName));

    private static bool StartsWithByteOrderMark(FileInfo file)
    {
        Span<byte> start = stackalloc byte[3];
        using var stream = file.OpenRead();

        return stream.ReadAtLeast(start, start.Length, throwOnEndOfStream: false) == start.Length
            && start.SequenceEqual(Encoding.UTF8.Preamble);
    }
}
