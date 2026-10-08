// Copyright (c) .NET Foundation and contributors.
// Licensed under the MIT license. See LICENSE.
// Adapted from https://github.com/dotnet/msbuild/blob/6df695adfc78f70ed122e27378e56f55afccfefe/src/Utilities.UnitTests/CanonicalError_Tests.cs

using Altinn.Authorization.RepoCtl.Model.Checks;
using Altinn.Authorization.RepoCtl.MsBuild;

namespace Altinn.Authorization.RepoCtl.Tests.MsBuild;

public class MsBuildDiagnosticParserTests
{
    private static readonly DirectoryInfo PathRoot = new(Path.Combine(Path.GetTempPath(), "repoctl-parser-tests"));

    [Theory]
    [InlineData("csc.exe")]
    [InlineData("filename")]
    [InlineData("file.cs")]
    public void OriginWithoutSeparator_IsPreserved(string origin)
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, $"{origin}(1,2): error: Message", out var diagnostic).ShouldBeTrue();
        diagnostic.ShouldNotBeNull().Origin.ShouldNotBeNull().Name.ShouldBe(origin);
    }

    [Theory]
    [InlineData("src/filename")]
    [InlineData("src\\filename")]
    public void RelativeExtensionlessPath_IsResolved(string origin)
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, $"{origin}: error: Message [{origin}]", out var diagnostic).ShouldBeTrue();
        var result = diagnostic.ShouldNotBeNull();
        var expected = Path.Combine(PathRoot.FullName, "src", "filename");
        result.Origin.ShouldNotBeNull().Name.ShouldBe(expected);
        result.ProjectContext.ShouldNotBeNull().ProjectPath.ShouldBe(expected);
    }

    [Theory]
    [InlineData("/src/filename")]
    [InlineData("\\src\\filename")]
    [InlineData("\\\\server\\share\\filename")]
    [InlineData("C:\\src\\filename")]
    [InlineData("C:/src/filename")]
    [InlineData("C:src/filename")]
    public void RootedPath_IsPreserved(string origin)
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, $"{origin}: error: Message [{origin}]", out var diagnostic).ShouldBeTrue();
        var result = diagnostic.ShouldNotBeNull();
        result.Origin.ShouldNotBeNull().Name.ShouldBe(origin);
        result.ProjectContext.ShouldNotBeNull().ProjectPath.ShouldBe(origin);
    }

    [Fact]
    public void TryParse_MapsDiagnosticAndResolvesPaths()
    {
        MsBuildDiagnosticParser.TryParse(PathRoot,
            "src/../src/app.cs(2,3-8): compiler warning CS0168: Unused variable [projects/app.csproj::TargetFramework=net10.0]",
            out var diagnostic).ShouldBeTrue();

        var result = diagnostic.ShouldNotBeNull();
        result.Severity.ShouldBe(DiagnosticSeverity.Warning);
        result.Text.ShouldBe("Unused variable");
        result.Category.ShouldBe("compiler");
        result.Code.ShouldBe("CS0168");
        var origin = result.Origin.ShouldNotBeNull();
        origin.Name.ShouldBe(Path.Combine(PathRoot.FullName, "src", "app.cs"));
        origin.Start.ShouldBe(new LinePosition(2, 3));
        origin.End.ShouldBe(new LinePosition(2, 8));
        var context = result.ProjectContext.ShouldNotBeNull();
        context.ProjectPath.ShouldBe(Path.Combine(PathRoot.FullName, "projects", "app.csproj"));
        context.ProjectProperties.ShouldBe("TargetFramework=net10.0");
    }

    [Fact]
    public void TryParse_WithoutOptionalFields_UsesNull()
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, "error: Message", out var diagnostic).ShouldBeTrue();

        var result = diagnostic.ShouldNotBeNull();
        result.Text.ShouldBe("Message");
        result.Origin.ShouldBeNull();
        result.Category.ShouldBeNull();
        result.Code.ShouldBeNull();
        result.ProjectContext.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Build succeeded.")]
    [InlineData("This error is not a diagnostic")]
    public void TryParse_WithNonDiagnostic_ReturnsNull(string message)
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, message, out var diagnostic).ShouldBeFalse();
        diagnostic.ShouldBeNull();
    }

    [Fact]
    public void TryParse_ColumnWithoutLine_DropsColumn()
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, "foo.cs(,2): error: Message", out var diagnostic).ShouldBeTrue();

        var origin = diagnostic.ShouldNotBeNull().Origin.ShouldNotBeNull();
        origin.Start.ShouldBe(default);
        origin.End.ShouldBe(default);
    }

    [Fact]
    public void TryParse_MultilineRange_PreservesEndLine()
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, "foo.cs(2,3,4,5): error: Message", out var diagnostic).ShouldBeTrue();

        var origin = diagnostic.ShouldNotBeNull().Origin.ShouldNotBeNull();
        origin.Start.ShouldBe(new LinePosition(2, 3));
        origin.End.ShouldBe(new LinePosition(4, 5));
    }

    [Theory]
    [InlineData("Message [/repo/app.csproj]", "Message", "/repo/app.csproj", "")]
    [InlineData("Message [/repo/app.csproj::TargetFramework=net10.0]", "Message", "/repo/app.csproj", "TargetFramework=net10.0")]
    [InlineData("Message [C:\\My Repo\\app.csproj::TargetFramework=net10.0;Configuration=Debug]", "Message", "C:\\My Repo\\app.csproj", "TargetFramework=net10.0;Configuration=Debug")]
    [InlineData("Message [other] [/repo/app.csproj::a=b::c=d]", "Message [other]", "/repo/app.csproj", "a=b::c=d")]
    [InlineData("Message [/repo/app.csproj]   ", "Message", "/repo/app.csproj", "")]
    [InlineData("Message", "Message", "", "")]
    [InlineData("Message [/repo/app.csproj] more text", "Message [/repo/app.csproj] more text", "", "")]
    [InlineData("Message [/repo/app.csproj", "Message [/repo/app.csproj", "", "")]
    [InlineData("Message []", "Message []", "", "")]
    [InlineData("Message [::TargetFramework=net10.0]", "Message [::TargetFramework=net10.0]", "", "")]
    public void ProjectSuffix(string text, string expectedText, string project, string props)
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, $"foo.cs(1,2): error CS0246: {text}", out var parts).ShouldBeTrue();

        parts!.Text.ShouldBe(expectedText);
        parts!.ProjectContext?.ProjectPath.ShouldBe(string.IsNullOrEmpty(project) ? null : ExpectedPath(project));
        parts!.ProjectContext?.ProjectProperties.ShouldBe(string.IsNullOrEmpty(props) ? null : props);
    }

    [Theory]
    [InlineData(350)]
    [InlineData(375)]
    [InlineData(450)]
    public void ProjectSuffix_AfterRecombiningText(int textLength)
    {
        var text = new string('x', textLength);
        const string project = "/repo/app.csproj";
        const string props = "TargetFramework=net10.0";
        MsBuildDiagnosticParser.TryParse(PathRoot, $"foo.cs(1,2): error CS0246: {text} [{project}::{props}]", out var parts).ShouldBeTrue();

        parts!.Text.ShouldBe(text);
        parts!.ProjectContext?.ProjectPath.ShouldBe(project);
        parts!.ProjectContext?.ProjectProperties.ShouldBe(string.IsNullOrEmpty(props) ? null : props);
    }

    [Fact]
    public void ProjectSuffix_ClangGccFormat_LeavesTextUnchanged()
    {
        const string text = "undeclared identifier 'foo' [/repo/app.csproj::TargetFramework=net10.0]";
        MsBuildDiagnosticParser.TryParse(PathRoot, $"foo.cpp:1:2: error: {text}", out var parts).ShouldBeTrue();

        parts!.Text.ShouldBe(text);
        parts!.ProjectContext.ShouldBeNull();
    }

    [Theory]
    [InlineData("65535", 65535)]
    [InlineData("65536", 0)]
    [InlineData("2147483648", 0)]
    public void PositionValues_RespectUInt16Range(string value, ushort expected)
    {
        ValidateFileNameMultiLineColumnError(
            $"foo.cs({value},{value},{value},{value}):error TST0000:Text",
            "foo.cs", expected, expected, expected, expected,
            DiagnosticSeverity.Error, "TST0000", "Text");

        MsBuildDiagnosticParser.TryParse(PathRoot, $"foo.cpp:{value}:{value}: error: Text", out var parts).ShouldBeTrue();
        (parts!.Origin?.Start.Line ?? 0).ShouldBe((uint)expected);
        (parts!.Origin?.Start.Column ?? 0).ShouldBe((uint)expected);
    }

    [Fact]
    public void EmptyOrigin()
    {
        ValidateToolError(@"error CS0006: Metadata file 'C:\WINDOWS\Microsoft.NET\Framework\v1.2.21213\System.dll' could not be found", "", DiagnosticSeverity.Error, "CS0006", @"Metadata file 'C:\WINDOWS\Microsoft.NET\Framework\v1.2.21213\System.dll' could not be found");
    }

    [Fact]
    public void Alink()
    {
        // From AL.EXE
        ValidateToolError(@"ALINK: error AL1017: No target filename was specified", "ALINK", DiagnosticSeverity.Error, "AL1017", @"No target filename was specified");
    }

    [Fact]
    public void CscWithFilename()
    {
        // From CSC.EXE
        ValidateFileNameLineColumnError(@"foo.resx(2,1): error CS0116: A namespace does not directly contain members such as fields or methods", @"foo.resx", 2, 1, DiagnosticSeverity.Error, "CS0116", "A namespace does not directly contain members such as fields or methods");
        ValidateFileNameLineColumnError(@"Main.cs(17,20): warning CS0168: The variable 'foo' is declared but never used", @"Main.cs", 17, 20, DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");
    }

    [Fact]
    public void VbcWithFilename()
    {
        // From VBC.EXE
        ValidateFileNameLineError(@"C:\WINDOWS\Microsoft.NET\Framework\v1.2.x86fre\foo.resx(2) : error BC30188: Declaration expected.", @"C:\WINDOWS\Microsoft.NET\Framework\v1.2.x86fre\foo.resx", 2, DiagnosticSeverity.Error, "BC30188", "Declaration expected.");
    }

    [Fact]
    public void ClWithFilename()
    {
        // From CL.EXE
        ValidateFileNameLineError(@"foo.cpp(1) : error C2143: syntax error : missing ';' before '++'", @"foo.cpp", 1, DiagnosticSeverity.Error, "C2143", "syntax error : missing ';' before '++'");
    }

    [Fact]
    public void JscWithFilename()
    {
        // From JSC.EXE
        ValidateFileNameLineColumnError(@"foo.resx(2,1) : error JS1135: Variable 'blech' has not been declared", @"foo.resx", 2, 1, DiagnosticSeverity.Error, "JS1135", "Variable 'blech' has not been declared");
    }

    [Fact]
    public void LinkWithFilename()
    {
        // From Link.exe
        // Note that this is impossible to distinguish from a tool error without
        // actually looking at the disk to see if the given file is there.
        ValidateFileNameError(@"foo.cpp : fatal error LNK1106: invalid file or disk full: cannot seek to 0x5361", @"foo.cpp", DiagnosticSeverity.Error, "LNK1106", "invalid file or disk full: cannot seek to 0x5361");
    }

    [Fact]
    public void BscMake()
    {
        // From BSCMAKE.EXE
        ValidateToolError(@"BSCMAKE: error BK1510 : corrupt .SBR file 'foo.cpp'", "BSCMAKE", DiagnosticSeverity.Error, "BK1510", @"corrupt .SBR file 'foo.cpp'");
    }

    [Fact]
    public void CvtRes()
    {
        // From CVTRES.EXE
        ValidateToolError(@"CVTRES : warning CVT4001: machine type not specified; assumed X86", "CVTRES", DiagnosticSeverity.Warning, "CVT4001", @"machine type not specified; assumed X86");
        ValidateToolError(@"CVTRES : fatal error CVT1103: cannot read file", "CVTRES", DiagnosticSeverity.Error, "CVT1103", @"cannot read file");
    }

    [Fact]
    public void DumpBinWithFilename()
    {
        // From DUMPBIN.EXE (notice that an 'LNK' error is returned).
        ValidateFileNameError(@"foo.cpp : warning LNK4048: Invalid format file; ignored", @"foo.cpp", DiagnosticSeverity.Warning, "LNK4048", "Invalid format file; ignored");
    }


    [Fact]
    public void LibWithFilename()
    {
        // From LIB.EXE
        ValidateFileNameError(@"foo.cpp : fatal error LNK1106: invalid file or disk full: cannot seek to 0x5361", @"foo.cpp", DiagnosticSeverity.Error, "LNK1106", "invalid file or disk full: cannot seek to 0x5361");
    }

    [Fact]
    public void MlWithFilename()
    {
        // From ML.EXE
        ValidateFileNameLineError(@"bar.h(2) : error A2008: syntax error : lksdflksj", @"bar.h", 2, DiagnosticSeverity.Error, "A2008", "syntax error : lksdflksj");
        ValidateFileNameLineError(@"bar.h(2) : error A2088: END directive required at end of file", @"bar.h", 2, DiagnosticSeverity.Error, "A2088", "END directive required at end of file");
    }

    [Fact]
    public void VcDeployWithFilename()
    {
        // From VCDEPLOY.EXE
        ValidateToolError(@"vcdeploy : error VCD0041: IIS must be installed on this machine in order for this program to function correctly.", "vcdeploy", DiagnosticSeverity.Error, "VCD0041", @"IIS must be installed on this machine in order for this program to function correctly.");
    }

    [Fact]
    public void VCBuildError()
    {
        // From VCBUILD.EXE
        ValidateFileNameLineError(@"1>c:\temp\testprefast\testprefast\testprefast.cpp(12) : error C4996: 'sprintf' was declared deprecated", @"c:\temp\testprefast\testprefast\testprefast.cpp", 12, DiagnosticSeverity.Error, "C4996", "'sprintf' was declared deprecated");
        ValidateFileNameLineError(@"1234>c:\temp\testprefast\testprefast\testprefast.cpp(12) : error C4996: 'sprintf' was declared deprecated", @"c:\temp\testprefast\testprefast\testprefast.cpp", 12, DiagnosticSeverity.Error, "C4996", "'sprintf' was declared deprecated");
    }

    [Fact]
    public void FileNameLine()
    {
        ValidateFileNameMultiLineColumnError("foo.cpp(1):error TST0000:Text", "foo.cpp", 1, 0, 0, 0, DiagnosticSeverity.Error, "TST0000", "Text");
    }

    [Fact]
    public void FileNameLineLine()
    {
        ValidateFileNameMultiLineColumnError("foo.cpp(1-5):error TST0000:Text", "foo.cpp", 1, 0, 5, 0, DiagnosticSeverity.Error, "TST0000", "Text");
    }

    [Fact]
    public void FileNameLineCol()
    {
        ValidateFileNameMultiLineColumnError("foo.cpp(1,15):error TST0000:Text", "foo.cpp", 1, 15, 0, 0, DiagnosticSeverity.Error, "TST0000", "Text");
    }

    [Fact]
    public void FileNameLineColCol()
    {
        ValidateFileNameMultiLineColumnError("foo.cpp(1,15-25):error TST0000:Text", "foo.cpp", 1, 15, 0, 25, DiagnosticSeverity.Error, "TST0000", "Text");
    }

    [Theory]
    [InlineData("1,15,10", 1, 15, 25)]
    [InlineData("1,15,0", 1, 15, 15)]
    [InlineData("1,65530,5", 1, 65530, 65535)]
    [InlineData("1,65530,6", 1, 65530, 0)]
    [InlineData("1,15,65536", 1, 15, 0)]
    [InlineData("1,15,", 1, 15, 0)]
    [InlineData("1,,10", 1, 0, 0)]
    [InlineData("65536,65536,10", 0, 0, 0)]
    public void FileNameLineColLength(string location, ushort line, ushort column, ushort endColumn)
    {
        ValidateFileNameMultiLineColumnError(
            $"foo.cpp({location}):error TST0000:Text", "foo.cpp", line, column, 0, endColumn,
            DiagnosticSeverity.Error, "TST0000", "Text");
    }

    [Fact]
    public void FileNameLineColLineCol()
    {
        ValidateFileNameMultiLineColumnError("foo.cpp(1,15,2,25):error TST0000:Text", "foo.cpp", 1, 15, 2, 25, DiagnosticSeverity.Error, "TST0000", "Text");
    }

    [Fact]
    public void PathologicalFileNameWithParens()
    {
        // Pathological case, there is actually a file with () at the end (Doesn't work, treats the (1) as a line number anyway).
        ValidateFileNameMultiLineColumnError("PathologicalFile.txt(1):error TST0000:Text", "PathologicalFile.txt", 1, 0, 0, 0, DiagnosticSeverity.Error, "TST0000", "Text");
    }

    [Fact]
    public void OverflowTrimmingShouldNotDropChar()
    {
        // A devdiv build produced a huge message like this!
        string message = @"The name 'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX' does not exist in the current context";
        string error = @"test.cs(1,32): error CS0103: " + message;

        MsBuildDiagnosticParser.TryParse(PathRoot, error, out var parts).ShouldBeTrue();

        parts!.Text.ShouldBe(message);
    }

    [Fact]
    public void ValidateErrorMessageWithFileName()
    {
        ValidateFileNameError("error CS2011: Error opening response file 'e:\foo\test.rsp' -- 'The device is not ready. '",
            "", DiagnosticSeverity.Error, "CS2011", "Error opening response file 'e:\foo\test.rsp' -- 'The device is not ready. '");
    }

    [Fact]
    public void ValidateErrorMessageWithFileName2()
    {
        ValidateToolError(@"BUILDMSG: error: Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.",
            "BUILDMSG", DiagnosticSeverity.Error, "", @"Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.");

        ValidateToolError(@"BUILDMSG: error : Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.",
            "BUILDMSG", DiagnosticSeverity.Error, "", @"Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.");
    }

    [Fact]
    public void ValidateErrorMessageWithFileName3()
    {
        ValidateNormalMessage(@"BUILDMSG: errorgarbage: Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.");

        ValidateNormalMessage(@"BUILDMSG: errorgarbage : Path 'c:\binaries.x86chk\bin\i386\System.AddIn.Contract.dll' is not under client's root 'c:\vstamq'.");
    }

    [Fact]
    public void ValidateErrorMessageVariableNotUsed()
    {
        // (line)
        ValidateFileNameMultiLineColumnError("Main.cs():Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        // This one actually falls under the (line-line) category. I'm not going to tweak the regex for this incorrect input just so we can
        // pretend -3 == 0, and just leaving it here for completeness
        ValidateFileNameMultiLineColumnError("Main.cs(-3):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 3, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        // (line-line)
        ValidateFileNameMultiLineColumnError("Main.cs(-):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(-2):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 2, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(1-):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 1, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        // (line,col)
        ValidateFileNameMultiLineColumnError("Main.cs(,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(,2):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 2, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(1,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 1, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        // Similarly to the previous odd case, this really falls under (line,col-col). Included for completeness, even if results are
        // not intuitive
        ValidateFileNameMultiLineColumnError("Main.cs(,-2):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 2,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(-1,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        // (line,col-col)
        ValidateFileNameMultiLineColumnError("Main.cs(,-):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(2,-):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 2, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(,4-):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 4, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(,-6):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 6,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(-1,-):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        // (line,col,line,col)
        ValidateFileNameMultiLineColumnError("Main.cs(,,,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(2,,,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 2, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(,3,,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 3, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(,,4,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 4, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(,,,5):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 5,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        // negative numbers are not matched at all for this format and I don't think we should tweak regexes to accept invalid input
        // in that form
        ValidateFileNameMultiLineColumnError("Main.cs(-2,,1,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(,-3,,2):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(3,,-4,):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");

        ValidateFileNameMultiLineColumnError("Main.cs(,4,,-5):Command line warning CS0168: The variable 'foo' is declared but never used",
            "Main.cs", 0, 0, 0, 0,
            DiagnosticSeverity.Warning, "CS0168", "The variable 'foo' is declared but never used");
    }

    [Fact]
    public void ClangGccError()
    {
        MsBuildDiagnosticParser.TryParse(PathRoot,
            "err.cpp:6:3: error: use of undeclared identifier 'force_an_error'", out var errorParts).ShouldBeTrue();

        errorParts!.Origin.ShouldNotBeNull().Name.ShouldBe("err.cpp");
        errorParts!.Severity.ShouldBe(DiagnosticSeverity.Error);
        errorParts!.Code.ShouldNotBeNull().ShouldStartWith("G");
        errorParts!.Code!.Length.ShouldBe(9);
        errorParts!.Text.ShouldBe("use of undeclared identifier 'force_an_error'");
        (errorParts!.Origin?.Start.Line ?? 0).ShouldBe((uint)6);
        (errorParts!.Origin?.Start.Column ?? 0).ShouldBe((uint)3);
        (errorParts!.Origin?.End.Line ?? 0).ShouldBe((uint)0);
        (errorParts!.Origin?.End.Column ?? 0).ShouldBe((uint)0);
    }

    [Fact]
    public void ClangGccErrorWithEmptyText()
    {
        ValidateFileNameMultiLineColumnError(
            "tests\\Tests\\Syntax.hs:1:1: error:",
            "tests\\Tests\\Syntax.hs",
            1, 1,
            0,
            0,
            DiagnosticSeverity.Error,
            "G00000000",
            string.Empty);
    }

    private static void ValidateToolError(string message, string tool, DiagnosticSeverity severity, string code, string text)
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, message, out var errorParts).ShouldBeTrue();

        errorParts!.Origin?.Name.ShouldBe(string.IsNullOrEmpty(tool) ? null : tool);
        errorParts!.Severity.ShouldBe(severity);
        errorParts!.Code.ShouldBe(string.IsNullOrEmpty(code) ? null : code);
        errorParts!.Text.ShouldBe(text);
        (errorParts!.Origin?.Start.Line ?? 0).ShouldBe((uint)0);
        (errorParts!.Origin?.Start.Column ?? 0).ShouldBe((uint)0);
        (errorParts!.Origin?.End.Line ?? 0).ShouldBe((uint)0);
        (errorParts!.Origin?.End.Column ?? 0).ShouldBe((uint)0);
    }

    private static void ValidateFileNameMultiLineColumnError(string message, string filename, ushort line, ushort column, ushort endLine, ushort endColumn, DiagnosticSeverity severity, string code, string text)
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, message, out var errorParts).ShouldBeTrue();

        errorParts!.Origin?.Name.ShouldBe(string.IsNullOrEmpty(filename) ? null
            : filename.Contains('/') || filename.Contains('\\') ? ExpectedPath(filename) : filename);
        errorParts!.Severity.ShouldBe(severity);
        errorParts!.Code.ShouldBe(string.IsNullOrEmpty(code) ? null : code);
        errorParts!.Text.ShouldBe(text);
        (errorParts!.Origin?.Start.Line ?? 0).ShouldBe((uint)line);
        (errorParts!.Origin?.Start.Column ?? 0).ShouldBe(line == 0 ? 0u : column);
        (errorParts!.Origin?.End.Line ?? 0).ShouldBe((uint)(endLine != 0 ? endLine : endColumn != 0 ? line : 0));
        (errorParts!.Origin?.End.Column ?? 0).ShouldBe(endLine == 0 && line == 0 ? 0u : endColumn);
    }

    private static void ValidateFileNameLineColumnError(string message, string filename, ushort line, ushort column, DiagnosticSeverity severity, string code, string text)
    {
        ValidateFileNameMultiLineColumnError(message, filename, line, column, 0, 0, severity, code, text);
    }

    private static void ValidateFileNameLineError(string message, string filename, ushort line, DiagnosticSeverity severity, string code, string text)
    {
        ValidateFileNameMultiLineColumnError(message, filename, line, 0, 0, 0, severity, code, text);
    }

    private static void ValidateFileNameError(string message, string filename, DiagnosticSeverity severity, string code, string text)
    {
        ValidateFileNameMultiLineColumnError(message, filename, 0, 0, 0, 0, severity, code, text);
    }

    private static string ExpectedPath(string path)
    {
        if (path.StartsWith(@"\\", StringComparison.Ordinal)
            || (path.Length >= 3 && path[1] == ':' && (path[2] == '\\' || path[2] == '/')))
        {
            return path;
        }

        return Path.GetFullPath(path.Replace('\\', Path.DirectorySeparatorChar), PathRoot.FullName);
    }

    private static void ValidateNormalMessage(string message)
    {
        MsBuildDiagnosticParser.TryParse(PathRoot, message, out var diagnostic).ShouldBeFalse();
        diagnostic.ShouldBeNull();
    }
}
