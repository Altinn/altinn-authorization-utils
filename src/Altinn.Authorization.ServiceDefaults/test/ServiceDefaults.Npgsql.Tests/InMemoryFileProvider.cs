using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

internal sealed class InMemoryFileProvider
    : IFileProvider
{
    private static readonly ImmutableArray<char> _pathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    public InMemoryDirectory Root { get; } = new("$ROOT", "/");

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        var entry = FindEntry(subpath);
        if (entry is InMemoryDirectory dir)
        {
            return dir;
        }

        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
        => FindEntry(subpath) as IFileInfo ?? new NotFoundFileInfo(subpath);

    private InMemoryFileEntry? FindEntry(string subpath)
    {
        if (string.IsNullOrEmpty(subpath))
        {
            return Root;
        }

        if (PathUtils.HasInvalidPathChars(subpath))
        {
            return null;
        }

        // Relative paths starting with leading slashes are okay
        var span = subpath.AsSpan();
        span = span.TrimStart(_pathSeparators.AsSpan());

        // Absolute paths are not allowed
        if (Path.IsPathRooted(span))
        {
            return null;
        }

        const int MAX_RANGES = 8;
        Span<Range> ranges = stackalloc Range[MAX_RANGES];
        InMemoryDirectory? dir = Root;

        while (true)
        {
            var segmentCount = span.SplitAny(ranges, _pathSeparators.AsSpan(), StringSplitOptions.RemoveEmptyEntries);
            var segmentRanges = ranges.Slice(0, Math.Min(segmentCount, MAX_RANGES - 1));

            InMemoryFileEntry? entry = dir;
            foreach (var range in segmentRanges)
            {
                if (dir is null)
                {
                    return null;
                }

                var name = span[range];
                entry = dir.GetEntry(name);
                if (entry is null)
                {
                    return null;
                }

                dir = entry as InMemoryDirectory;
            }

            if (segmentCount < MAX_RANGES)
            {
                return entry;
            }

            span = span[ranges[^1]];
        }
    }

    public IChangeToken Watch(string filter)
    {
        throw new NotSupportedException();
    }

    public abstract class InMemoryFileEntry(string name, string path)
        : IFileInfo
    {
        bool IFileInfo.Exists => true;

        public abstract bool IsDirectory { get; }

        DateTimeOffset IFileInfo.LastModified { get; } = DateTimeOffset.UtcNow;

        protected abstract long Length { get; }

        long IFileInfo.Length => Length;

        public string Name { get; } = name;

        public string Path { get; } = path;

        string? IFileInfo.PhysicalPath => null;

        protected abstract Stream CreateReadStream();

        Stream IFileInfo.CreateReadStream() => CreateReadStream();
    }

    [DebuggerDisplay("Directory: {Path}")]
    public sealed class InMemoryDirectory(string name, string path)
        : InMemoryFileEntry(name, path)
        , IDirectoryContents
    {
        private readonly Dictionary<string, InMemoryFileEntry> _entries = new();

        public override bool IsDirectory => true;

        bool IDirectoryContents.Exists => true;

        protected override long Length => -1;

        public InMemoryFileEntry? GetEntry(ReadOnlySpan<char> name)
        {
            return _entries.TryGetValue(name.ToString(), out var entry) ? entry : null;
        }

        public InMemoryDirectory CreateSubdirectory(string name)
        {
            if (_entries.TryGetValue(name, out var entry))
            {
                if (entry is InMemoryDirectory dir)
                {
                    return dir;
                }

                ThrowHelper.ThrowInvalidOperationException("Directory already contains file with that name.");
            }

            var newDir = new InMemoryDirectory(name, $"{Path}{name}/");
            _entries.Add(name, newDir);
            return newDir;
        }

        public InMemoryFile CreateFile(string name, byte[] contents)
        {
            if (name.Contains('/'))
            {
                var segments = name.Split('/');
                name = segments[^1];

                var dir = this;
                for (var i = 0; i < segments.Length - 1; i++)
                {
                    dir = dir.CreateSubdirectory(segments[i]);
                }

                return dir.CreateFile(name, contents);
            }

            if (_entries.ContainsKey(name))
            {
                ThrowHelper.ThrowInvalidOperationException("File already exists.");
            }

            var file = new InMemoryFile(name, $"{Path}{name}", contents);
            _entries.Add(name, file);
            return file;
        }

        public InMemoryFile CreateFile(string name, string content)
            => CreateFile(name, Encoding.UTF8.GetBytes(content));

        protected override Stream CreateReadStream()
            => ThrowHelper.ThrowNotSupportedException<Stream>("Cannot read a directory");

        IEnumerator<IFileInfo> IEnumerable<IFileInfo>.GetEnumerator()
            => _entries.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<IFileInfo>)this).GetEnumerator();
    }

    [DebuggerDisplay("File: {Path}")]
    public sealed class InMemoryFile(string name, string path, byte[] contents)
        : InMemoryFileEntry(name, path)
    {
        private readonly byte[] _contents = contents;

        public override bool IsDirectory => false;

        protected override long Length => _contents.Length;

        protected override Stream CreateReadStream()
            => new MemoryStream(_contents, writable: false);
    }

    private static class PathUtils
    {
        private static char[] GetInvalidFileNameChars() => Path.GetInvalidFileNameChars()
            .Where(c => c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar).ToArray();

        private static readonly SearchValues<char> _invalidFileNameChars = SearchValues.Create(GetInvalidFileNameChars());

        internal static bool HasInvalidPathChars(string path)
            => path.AsSpan().ContainsAny(_invalidFileNameChars);
    }
}
