﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Altinn.Urn.SourceGenerator.Tests;

public static class SourceGeneratorUtils
{
    public static async Task VerifySourceGeneratorOutput(string source, DiagnosticDescriptor[] expectedDiagnostics, string fileName = "Test.cs")
    {
        ModuleInitializer.Init();

        var sourceFile = SourceText.From(source);

        // Create a compilation for the source code
        var compilation = await BaseCompilation.Value;
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(sourceFile, new CSharpParseOptions(compilation.LanguageVersion)));

        // Create an instance of our generator
        var generator = new UrnGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generator and get the results
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation!, out _, out _);

        // Use verify to snapshot the generated code
        await Verifier.Verify(driver);

        var result = driver.GetRunResult();
        
        if (expectedDiagnostics.Length == 0)
        {
            result.Diagnostics.ShouldBeEmpty();
            return;
        }

        var remaining = new HashSet<DiagnosticDescriptor>(expectedDiagnostics);
        foreach (var diagnostic in result.Diagnostics)
        {
            expectedDiagnostics.ShouldContain(diagnostic.Descriptor);
            remaining.Remove(diagnostic.Descriptor);
        }

        remaining.ShouldBeEmpty("All expected exceptions should be present");
    }

    private static readonly Lazy<Task<CSharpCompilation>> BaseCompilation = new(async () =>
    {
        var workspace = MSBuildWorkspace.Create();
        var projectFile = Path.Combine(
            Path.GetDirectoryName(Path.GetDirectoryName(FindUp("Altinn.Urn.SourceGenerator.Tests.csproj")))!,
            "Altinn.Urn.SourceGenerator.IntegrationTests",
            "Altinn.Urn.SourceGenerator.IntegrationTests.csproj");
        var project = await workspace.OpenProjectAsync(projectFile);

        project = project
            .WithAnalyzerReferences([]);

        var compilation = await project.GetCompilationAsync();
        if (compilation == null)
        {
            throw new Exception("Failed to get compilation");
        }

        compilation = compilation.RemoveAllSyntaxTrees();

        return (CSharpCompilation)compilation;
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    private static string FindUp(string fileName)
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            var file = Path.Combine(dir, fileName);
            if (File.Exists(file))
            {
                return file;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new FileNotFoundException($"Could not find file '{fileName}'");
    }
}
