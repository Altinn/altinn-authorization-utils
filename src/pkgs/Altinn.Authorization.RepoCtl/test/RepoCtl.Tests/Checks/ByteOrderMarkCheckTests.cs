using System.Text;
using Altinn.Authorization.RepoCtl.Checks;

namespace Altinn.Authorization.RepoCtl.Tests.Checks;

public sealed class ByteOrderMarkCheckTests
    : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("repoctl-bom-check-tests-");

    public void Dispose()
        => _root.Delete(recursive: true);

    [Fact]
    public void BruFileWithBom_IsReported()
    {
        var path = WriteFile("src/apps/App/test/Bruno/request.bru", bom: true);

        var diagnostic = ByteOrderMarkCheck.Check(_root).ShouldHaveSingleItem();

        diagnostic.Code.ShouldBe("REPOCTL016");
        diagnostic.Origin.ShouldNotBeNull().Name.ShouldBe(path);
    }

    [Fact]
    public void BruFileWithoutBom_IsNotReported()
    {
        WriteFile("src/apps/App/test/Bruno/request.bru", bom: false);

        ByteOrderMarkCheck.Check(_root).ShouldBeEmpty();
    }

    [Fact]
    public void OtherFileWithBom_IsNotReported()
    {
        WriteFile("src/apps/App/src/Program.cs", bom: true);

        ByteOrderMarkCheck.Check(_root).ShouldBeEmpty();
    }

    private string WriteFile(string relativePath, bool bom)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root.FullName, relativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "meta {\n}\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: bom));

        return fullPath;
    }
}
