using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model.ReleasePlease;

/// <summary>
/// Release-please release strategies.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ReleasePleaseReleaseType>))]
public enum ReleasePleaseReleaseType
{
    /// <summary>Indicates that the changelog type has not been set.</summary>
    /// <remarks>In reality, this should never be set by JSON, but it's easier to model this way</remarks>
    [JsonStringEnumMemberName("unset")]
    Unset,

    /// <summary>Updates a Bazel module's <c>MODULE.bazel</c> file.</summary>
    [JsonStringEnumMemberName("bazel")]
    Bazel,

    /// <summary>Updates a Dart package's <c>pubspec.yaml</c> file.</summary>
    [JsonStringEnumMemberName("dart")]
    Dart,

    /// <summary>Updates an Elixir project's <c>mix.exs</c> file.</summary>
    [JsonStringEnumMemberName("elixir")]
    Elixir,

    /// <summary>Updates an Expo project's <c>package.json</c> and <c>app.json</c> files.</summary>
    [JsonStringEnumMemberName("expo")]
    Expo,

    /// <summary>Creates a Go release and changelog.</summary>
    [JsonStringEnumMemberName("go")]
    Go,

    /// <summary>Uses the Go Librarian release strategy.</summary>
    [JsonStringEnumMemberName("go-librarian")]
    GoLibrarian,

    /// <summary>Uses Google's Go Yoshi repository release strategy.</summary>
    [JsonStringEnumMemberName("go-yoshi")]
    GoYoshi,

    /// <summary>Updates a Helm chart's <c>Chart.yaml</c> file.</summary>
    [JsonStringEnumMemberName("helm")]
    Helm,

    /// <summary>Updates Java build files and manages post-release snapshot versions.</summary>
    [JsonStringEnumMemberName("java")]
    Java,

    /// <summary>Uses the Java backport release strategy.</summary>
    [JsonStringEnumMemberName("java-backport")]
    JavaBackport,

    /// <summary>Uses the Java bill-of-materials release strategy.</summary>
    [JsonStringEnumMemberName("java-bom")]
    JavaBom,

    /// <summary>Uses the Java long-term-support release strategy.</summary>
    [JsonStringEnumMemberName("java-lts")]
    JavaLts,

    /// <summary>Uses Google's Java Yoshi repository release strategy.</summary>
    [JsonStringEnumMemberName("java-yoshi")]
    JavaYoshi,

    /// <summary>Uses Google's Java Yoshi monorepo release strategy.</summary>
    [JsonStringEnumMemberName("java-yoshi-mono-repo")]
    JavaYoshiMonoRepo,

    /// <summary>Updates a KRM blueprint's package files.</summary>
    [JsonStringEnumMemberName("krm-blueprint")]
    KrmBlueprint,

    /// <summary>Updates Maven <c>pom.xml</c> files and manages post-release snapshot versions.</summary>
    [JsonStringEnumMemberName("maven")]
    Maven,

    /// <summary>Updates a Node package's <c>package.json</c> file.</summary>
    [JsonStringEnumMemberName("node")]
    Node,

    /// <summary>Uses the Node Librarian release strategy.</summary>
    [JsonStringEnumMemberName("node-librarian")]
    NodeLibrarian,

    /// <summary>Updates OCaml package metadata such as <c>opam</c> or <c>esy.json</c>.</summary>
    [JsonStringEnumMemberName("ocaml")]
    Ocaml,

    /// <summary>Updates a PHP package's <c>composer.json</c> file.</summary>
    [JsonStringEnumMemberName("php")]
    Php,

    /// <summary>Uses the PHP Librarian release strategy.</summary>
    [JsonStringEnumMemberName("php-librarian")]
    PhpLibrarian,

    /// <summary>Uses Google's PHP Yoshi repository release strategy.</summary>
    [JsonStringEnumMemberName("php-yoshi")]
    PhpYoshi,

    /// <summary>Updates Python package version files.</summary>
    [JsonStringEnumMemberName("python")]
    Python,

    /// <summary>Uses the Python Librarian release strategy.</summary>
    [JsonStringEnumMemberName("python-librarian")]
    PythonLibrarian,

    /// <summary>Updates an R package's version files.</summary>
    [JsonStringEnumMemberName("r")]
    R,

    /// <summary>Updates a Ruby package's version file.</summary>
    [JsonStringEnumMemberName("ruby")]
    Ruby,

    /// <summary>Uses the Ruby Librarian release strategy.</summary>
    [JsonStringEnumMemberName("ruby-librarian")]
    RubyLibrarian,

    /// <summary>Uses Google's Ruby Yoshi repository release strategy.</summary>
    [JsonStringEnumMemberName("ruby-yoshi")]
    RubyYoshi,

    /// <summary>Updates Rust crate or workspace <c>Cargo.toml</c> files.</summary>
    [JsonStringEnumMemberName("rust")]
    Rust,

    /// <summary>Uses the Salesforce DX release strategy.</summary>
    [JsonStringEnumMemberName("salesforce")]
    Salesforce,

    /// <summary>Updates a Salesforce DX project's <c>sfdx-project.json</c> file.</summary>
    [JsonStringEnumMemberName("sfdx")]
    Sfdx,

    /// <summary>Updates a plain-text version file. The default file is <c>version.txt</c>.</summary>
    [JsonStringEnumMemberName("simple")]
    Simple,

    /// <summary>Updates a Terraform module's documented version.</summary>
    [JsonStringEnumMemberName("terraform-module")]
    TerraformModule,

    /// <summary>Uses Google's .NET Yoshi repository release strategy.</summary>
    [JsonStringEnumMemberName("dotnet-yoshi")]
    DotnetYoshi,
}
