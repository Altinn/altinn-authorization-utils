using System.Text;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Checks;
using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Tests.Checks;

public sealed class ByteOrderMarkCheckTests
    : IAsyncLifetime
{
    private static readonly byte[] Bom = Encoding.UTF8.Preamble.ToArray();
    private static readonly byte[] Content = Encoding.UTF8.GetBytes("meta {\n  name: Test\n}\n");

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("repoctl-bom-check-tests-");

    public async ValueTask InitializeAsync()
    {
        await Git("init", "--quiet");
    }

    public ValueTask DisposeAsync()
    {
        // git marks objects read-only, which Directory.Delete does not handle on Windows.
        foreach (var file in _root.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            file.Attributes = FileAttributes.Normal;
        }

        _root.Delete(recursive: true);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task TrackedBruFileWithBom_IsReported()
    {
        var path = await WriteFile("collection/folder with space/request.bru", bom: true, track: true);

        var diagnostics = await Check();

        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.Code.ShouldBe("REPOCTL016");
        diagnostic.Origin.ShouldNotBeNull().Name.ShouldBe(path);
        diagnostic.Origin.Start.Line.ShouldBe(1u);
        diagnostic.Text.ShouldContain("Bruno");
    }

    [Fact]
    public async Task EveryTrackedBruFileWithBom_IsReported()
    {
        var first = await WriteFile("a/first.bru", bom: true, track: true);
        var second = await WriteFile("b/c/second.bru", bom: true, track: true);
        await WriteFile("b/c/third.bru", bom: false, track: true);

        var diagnostics = await Check();

        diagnostics.Select(static d => d.Origin!.Name).ShouldBe(new[] { first, second }, ignoreOrder: true);
    }

    [Fact]
    public async Task TrackedBruFileWithoutBom_IsNotReported()
    {
        await WriteFile("request.bru", bom: false, track: true);

        var diagnostics = await Check();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task UntrackedBruFileWithBom_IsNotReported()
    {
        await WriteFile("request.bru", bom: true, track: false);

        var diagnostics = await Check();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task OtherFileWithBom_IsNotReported()
    {
        await WriteFile("Program.cs", bom: true, track: true);

        var diagnostics = await Check();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task EmptyAndShortBruFiles_AreNotReported()
    {
        File.WriteAllBytes(Path.Combine(_root.FullName, "empty.bru"), []);
        File.WriteAllBytes(Path.Combine(_root.FullName, "short.bru"), Bom.AsSpan(0, 2).ToArray());
        await Git("add", "--", "empty.bru", "short.bru");

        var diagnostics = await Check();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task NotAGitRepository_ReportsExternalToolFailure()
    {
        var notARepo = Directory.CreateTempSubdirectory("repoctl-bom-check-tests-no-git-");
        try
        {
            // The temp directory is not inside a git working tree.
            var diagnostics = await new ByteOrderMarkCheck()
                .Check(notARepo, TestContext.Current.CancellationToken)
                .ToListAsync(TestContext.Current.CancellationToken);

            var diagnostic = diagnostics.ShouldHaveSingleItem();
            diagnostic.Code.ShouldBe("REPOCTL014");
        }
        finally
        {
            notARepo.Delete(recursive: true);
        }
    }

    private async Task<List<Diagnostic>> Check()
        => await new ByteOrderMarkCheck()
            .Check(_root, TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

    private async Task<string> WriteFile(string relativePath, bool bom, bool track)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root.FullName, relativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, bom ? [.. Bom, .. Content] : Content, TestContext.Current.CancellationToken);

        if (track)
        {
            await Git("add", "--", relativePath);
        }

        return fullPath;
    }

    private Task Git(params string[] args)
        => Command.Create("git", args)
            .WithWorkingDirectory(_root.FullName)
            .ExecuteCaptured(TestContext.Current.CancellationToken);
}
