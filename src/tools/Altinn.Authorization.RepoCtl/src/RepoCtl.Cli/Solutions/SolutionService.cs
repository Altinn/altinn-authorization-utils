using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Altinn.Authorization.ModelUtils;
using Altinn.Authorization.RepoCtl.Checks;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Utils;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;

namespace Altinn.Authorization.RepoCtl.Solutions;

internal sealed partial class SolutionService(ILogger<SolutionService> logger)
    : IRepositoryCheck
{
    private static readonly Encoding Utf8EncodingWithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public async Task UpdateSolutions(AltinnRepository repository, CancellationToken cancellationToken)
    {
        foreach (var (solutionFile, solution) in GetSolutions(repository))
        {
            await UpdateSolution(solutionFile, solution, cancellationToken);
            Log.WroteSolutionFile(logger, GetRelativePath(repository.RootDirectory.FullName, solutionFile.FullName));
        }
    }

    string IRepositoryCheck.CheckId => "slnx-files";
    string IRepositoryCheck.CheckDisplayName => "Solution files";

    public async IAsyncEnumerable<CheckIssue> Check(AltinnRepository repository, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var (solutionFile, solution) in GetSolutions(repository))
        {
            CheckIssue? issue = null;
            try
            {
                await using var fs = solutionFile.OpenRead();
                using var wanted = new Sequence<byte>(ArrayPool<byte>.Shared);
                WriteSolutionContents(wanted, solution);

                using var actual = new Sequence<byte>(ArrayPool<byte>.Shared);
                await fs.NormalizeLineEndingsAndCopyToAsync(actual, cancellationToken);

                if (!wanted.AsReadOnlySequence.SequenceEqual(actual.AsReadOnlySequence))
                {
                    issue = CheckIssue.CreateFile(GetRelativePath(repository.RootDirectory.FullName, solutionFile.FullName), "Solution file is out of date.");
                }
            }
            catch (FileNotFoundException)
            {
                issue = CheckIssue.CreateFile(GetRelativePath(repository.RootDirectory.FullName, solutionFile.FullName), "Solution file does not exist.");
            }

            if (issue is not null)
            {
                yield return issue;
            }
        }
    }

    private IEnumerable<(FileInfo File, Solution Solution)> GetSolutions(AltinnRepository repository)
    {
        yield return (repository.SolutionFile, CreateRootSolution(repository));
        foreach (var vertical in repository.Verticals)
        {
            yield return (vertical.SolutionFile, CreateVerticalSolution(vertical));
        }
    }

    private async Task UpdateSolution(FileInfo solutionFile, Solution solution, CancellationToken cancellationToken)
    {
        using var seq = new Sequence<byte>(ArrayPool<byte>.Shared);
        WriteSolutionContents(seq, solution);

        await using var fileStream = solutionFile.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.WriteAsync(seq.AsReadOnlySequence, cancellationToken);
    }

    private void WriteSolutionContents(IBufferWriter<byte> writer, Solution solution)
    {
        {
            using var xmlWriter = XmlWriter.Create(writer.AsStream(), new XmlWriterSettings
            {
                Async = false,
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
                Encoding = Utf8EncodingWithoutBom,
                OmitXmlDeclaration = true,
            });

            solution.WriteTo(xmlWriter);
        }

        writer.Write("\n"u8); // trailing newline
    }

    private Solution CreateRootSolution(AltinnRepository repository)
    {
        var rootPath = repository.RootDirectory.FullName;
        var byKind = repository.Verticals.AsEnumerable()
            .GroupBy(static v => v.Kind)
            .OrderBy(static g => g.Key)
            .Select(g => CreateKindFolder(g.Key, g, rootPath))
            .ToList();

        if (byKind.Count == 0)
        {
            throw new InvalidOperationException("The repository contains no verticals.");
        }

        if (byKind.Count == 1)
        {
            return new Solution(byKind[0]);
        }

        var rootBuilder = Solution.Folder.CreateBuilder(repository.Name);
        rootBuilder.AddRange(byKind);
        return new Solution(rootBuilder.Build());

        static Solution.Folder CreateKindFolder(AltinnVerticalKind kind, IEnumerable<AltinnVertical> verticals, string rootPath)
        {
            var builder = Solution.Folder.CreateBuilder(kind.ToString("dir", formatProvider: null));

            foreach (var vertical in verticals)
            {
                builder.Add(CreateVerticalFolder(vertical, rootPath, includeDeps: false));
            }

            return builder.Build();
        }
    }

    private Solution CreateVerticalSolution(AltinnVertical vertical)
    {
        var rootPath = vertical.Directory.FullName;
        var folder = CreateVerticalFolder(vertical, rootPath, includeDeps: true);
        return new Solution(folder);
    }

    private static Solution.Folder CreateVerticalFolder(AltinnVertical vertical, string rootPath, bool includeDeps)
    {
        Solution.Folder.Builder? deps = null;
        Solution.Folder.Builder? samples = null;
        Solution.Folder.Builder? srcs = null;
        Solution.Folder.Builder? tests = null;
        Solution.Folder.Builder? testLibs = null;

        if (includeDeps)
        {
            foreach (var dep in vertical.AllBuildDependencies)
            {
                deps ??= Solution.Folder.CreateBuilder("deps");
                var depDir = Solution.Folder.CreateBuilder(dep.DisplayName);

                foreach (var project in dep.Projects.Where(static p => p.Type.CanBeReferencedByOtherProjects))
                {
                    depDir.Add(CreateProject(project, rootPath));
                }

                deps.Add(depDir.Build());
            }
        }

        foreach (var project in vertical.Projects)
        {
            if (project.Type.IsSampleProject)
            {
                samples ??= Solution.Folder.CreateBuilder("sample");
                samples.Add(CreateProject(project, rootPath));
            }
            else if (project.Type.IsTestLib)
            {
                testLibs ??= Solution.Folder.CreateBuilder("lib");
                testLibs.Add(CreateProject(project, rootPath));
            }
            else if (project.Type.IsTestProject)
            {
                tests ??= Solution.Folder.CreateBuilder("test");
                tests.Add(CreateProject(project, rootPath));
            }
            else
            {
                srcs ??= Solution.Folder.CreateBuilder("src");
                srcs.Add(CreateProject(project, rootPath));
            }
        }

        if (testLibs is not null)
        {
            tests ??= Solution.Folder.CreateBuilder("test");
            tests.Add(testLibs.Build());
        }

        var builder = Solution.Folder.CreateBuilder(vertical.FullName);
        builder.AddOpt(deps?.Build());
        builder.AddOpt(samples?.Build());
        builder.AddOpt(srcs?.Build());
        builder.AddOpt(tests?.Build());

        return builder.Build();
    }

    private static Solution.Project CreateProject(AltinnProject project, string rootPath)
    {
        var relativePath = GetRelativePath(rootPath, project.ProjectFile.FullName);
        return new Solution.Project(relativePath);
    }

    private static string GetRelativePath(string rootPath, string fullPath)
    {
        var relativePath = Path.GetRelativePath(rootPath, fullPath);
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    private sealed record Solution
    {
        private readonly Folder _root;

        public Solution(Folder root)
        {
            _root = root;
        }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("Solution");
            _root.WriteTo(writer, path: "/", skipRootFolder: true);
            writer.WriteEndElement();
        }

        internal sealed record Folder
        {
            public static Builder CreateBuilder(string name) => new(name);

            private Folder(string name, ImmutableValueArray<Folder> folders, ImmutableValueArray<Project> projects)
            {
                Guard.IsNotNullOrWhiteSpace(name);
                if (folders.IsEmpty && projects.IsEmpty)
                {
                    throw new ArgumentException("A folder must contain at least one subfolder or project.");
                }

                if (name.StartsWith('/') || name.EndsWith('/'))
                {
                    throw new ArgumentException("Folder name must NOT start and end with a slash.");
                }

                Name = name;
                Folders = folders;
                Projects = projects;
            }

            public string Name { get; }
            public ImmutableValueArray<Folder> Folders { get; }
            public ImmutableValueArray<Project> Projects { get; }

            public void WriteTo(XmlWriter writer, string path, bool skipRootFolder = false)
            {
                var selfPath = $"{path}{Name}/";
                if (!skipRootFolder)
                {
                    writer.WriteStartElement("Folder");
                    writer.WriteAttributeString("Name", selfPath);
                }

                foreach (var project in Projects)
                {
                    project.WriteTo(writer);
                }

                if (!skipRootFolder)
                {
                    writer.WriteEndElement();
                }

                foreach (var folder in Folders)
                {
                    folder.WriteTo(writer, path: skipRootFolder ? path : selfPath);
                }
            }

            public sealed class Builder
            {
                private readonly string _name;
                private readonly ImmutableArray<Folder>.Builder _folders;
                private readonly ImmutableArray<Project>.Builder _projects;

                internal Builder(string name)
                {
                    _name = name;
                    _folders = ImmutableArray.CreateBuilder<Folder>();
                    _projects = ImmutableArray.CreateBuilder<Project>();
                }

                public Builder Add(Folder folder) { _folders.Add(folder); return this; }
                public Builder Add(Project project) { _projects.Add(project); return this; }

                public Builder AddOpt(Folder? folder) { if (folder is not null) _folders.Add(folder); return this; }
                public Builder AddOpt(Project? project) { if (project is not null) _projects.Add(project); return this; }

                public Builder AddRange(IEnumerable<Folder> folders) { _folders.AddRange(folders); return this; }
                public Builder AddRange(IEnumerable<Project> projects) { _projects.AddRange(projects); return this; }

                public Folder Build() => new Folder(_name, _folders.DrainToImmutableValueArray(), _projects.DrainToImmutableValueArray());
            }
        }

        internal sealed record Project
        {
            public Project(string relativePath)
            {
                Guard.IsNotNullOrWhiteSpace(relativePath);
                RelativePath = relativePath;
            }

            public string RelativePath { get; }

            public void WriteTo(XmlWriter writer)
            {
                writer.WriteStartElement("Project");
                writer.WriteAttributeString("Path", RelativePath);
                writer.WriteEndElement();
            }
        }
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Wrote solution file '{SolutionFile}'.")]
        public static partial void WroteSolutionFile(ILogger logger, string solutionFile);
    }
}
