using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Checks;

/// <summary>
/// Checks that files read by tools which cannot handle a UTF-8 byte order mark do not start with one.
/// </summary>
/// <remarks>
/// Only files tracked by git are checked, so build output and local files are ignored.
/// </remarks>
internal sealed class ByteOrderMarkCheck
    : IRepositoryCheck
{
    /// <summary>
    /// The files that must not start with a byte order mark, as git pathspecs, and why.
    /// </summary>
    internal static readonly ImmutableArray<(string Pathspec, string Reason)> Rules =
    [
        // The Bruno CLI parses the whole collection before it sends a request, so one such file fails the entire run.
        ("*.bru", "the Bruno CLI cannot parse a .bru file that starts with one"),
    ];

    /// <inheritdoc/>
    public string CheckId
        => "no-bom";

    /// <inheritdoc/>
    public string CheckDisplayName
        => "Files Without Byte Order Mark";

    /// <inheritdoc/>
    public IAsyncEnumerable<Diagnostic> Check(AltinnRepository repository, CancellationToken cancellationToken)
        => Check(repository.RootDirectory, cancellationToken);

    /// <summary>
    /// Checks the files tracked by git in <paramref name="rootDirectory"/>.
    /// </summary>
    /// <param name="rootDirectory">The root of the git working tree.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A diagnostic for each file that starts with a byte order mark.</returns>
    internal async IAsyncEnumerable<Diagnostic> Check(DirectoryInfo rootDirectory, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var (pathspec, reason) in Rules)
        {
            var command = Command.Create("git", "ls-files", "-z", "--", pathspec)
                .WithWorkingDirectory(rootDirectory.FullName);

            string output;
            bool processFailed = false;
            int? errorExitCode = null;
            try
            {
                (_, output, _) = await command.ExecuteCaptured(cancellationToken);
            }
            catch (ProcessFailedException e)
            {
                // we can't yield in catch
                using var process = e.Process;
                processFailed = true;
                errorExitCode = process?.ExitCode;
                output = string.Empty;
            }

            if (processFailed)
            {
                yield return Diagnostics.ExternalToolFailed(command.ToString("display"), errorExitCode);
                continue;
            }

            foreach (var relativePath in output.Split('\0', StringSplitOptions.RemoveEmptyEntries))
            {
                var fullPath = Path.GetFullPath(Path.Combine(rootDirectory.FullName, relativePath));
                if (await StartsWithByteOrderMark(fullPath, cancellationToken))
                {
                    yield return Diagnostics.FileStartsWithByteOrderMark(fullPath, reason);
                }
            }
        }
    }

    private static async Task<bool> StartsWithByteOrderMark(string fullPath, CancellationToken cancellationToken)
    {
        // A tracked file can be deleted in the working tree.
        if (!File.Exists(fullPath))
        {
            return false;
        }

        await using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 1, useAsync: true);
        var buffer = new byte[Encoding.UTF8.Preamble.Length];
        var read = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken);

        return read == buffer.Length && buffer.AsSpan().SequenceEqual(Encoding.UTF8.Preamble);
    }
}
