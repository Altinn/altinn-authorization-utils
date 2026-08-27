using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Formatting;
using Altinn.Authorization.RepoCtl;
using Altinn.Authorization.RepoCtl.Binding;
using Altinn.Authorization.RepoCtl.Checks;
using Altinn.Authorization.RepoCtl.Commands.Ci;
using Altinn.Authorization.RepoCtl.Formatting;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.NuGet;
using Altinn.Authorization.RepoCtl.Options;
using Altinn.Authorization.RepoCtl.Solutions;
using Altinn.Authorization.RepoCtl.Utils;
using Microsoft.Extensions.DependencyInjection;

var builder = CliApplication.CreateBuilder("Altinn Authorization Repository Manager (repoctl)");
builder.Services.AddGitHubActionsServices();
builder.Services.AddSingleton<AltinnRepositoryResolver>();
builder.Services.AddSingleton<IConfigureOption, ConfigureAltinnVerticalKindOptions>();
builder.Services.AddSingleton<ICommandHandlerParameterBinderResolver, AltinnRepositoryBinderResolver>();
builder.Services.AddSingleton<AltinnRepositoryLoader>();
builder.Services.AddSingleton<SolutionService>();
builder.Services.AddSingleton<TestService>();
builder.Services.AddSingleton<PackService>();
builder.Services.AddSingleton<GitHubService>();
builder.Services.AddSingleton<NuGetService>();
builder.Services.AddSingleton<Checker>();
builder.Services.AddSingleton<RepositoryChecker>();
builder.Services.AddSingleton<AltinnRepositoryAccessor>();
builder.Services.AddCommandResultHandlerResolver<CheckCommandResultHandlerResolver>();

builder.Services.AddSingleton<IRepositoryCheck>(s => s.GetRequiredService<SolutionService>());
builder.Services.AddSingleton<IRepositoryCheck, DotnetFormatCheck>();

builder.Services.AddOutputFormatter<RichFormat, AltinnVerticalSetFormatter>();
builder.Services.AddOutputFormatter<JsonFormat, AltinnVerticalSetFormatter>();
builder.Services.AddOutputFormatter<RichFormat, CheckRunFormatter>();
builder.Services.AddOutputFormatter<JsonFormat, CheckRunFormatter>();

var cli = builder.Build();
cli.ApplicationServices.GetRequiredService<AltinnRepositoryResolver>().Configure(cli);

cli.AddCommand("verticals", "Operate on verticals", (builder) =>
{
    builder.AddCommand("list", "List all verticals in the repository", (AltinnVerticalSet verticals) => verticals);
});

cli.AddCommand("vertical", "Operate on a single vertical", (builder) =>
{
    builder.AddCommand("test", "Run tests for the vertical (if it has any)", (AltinnVertical vertical, [RestArguments] string[] args, TestService testService)
        => testService.RunTests(vertical, args));

    builder.AddCommand("pack", "Pack the vertical into a package (if it is packable)", (AltinnVertical vertical, [RestArguments] string[] args, PackService packService)
        => packService.Pack(vertical, args));
});

cli.AddCommand("solutions", "Operate on solutions", (builder) =>
{
    builder.AddCommand("update", "Update the solutions in this repository", async (AltinnRepository repository, SolutionService solutionService, CancellationToken cancellationToken) =>
    {
        await solutionService.UpdateSolutions(repository, cancellationToken);
    });

    builder.AddCommand("check", "Check the solutions in this repository", (Checker checker, AltinnRepository repository, SolutionService solutionService) =>
    {
        return CheckCommandResult.Partial(checker, repository, [solutionService]);
    });
});

cli.AddCommand("check", "Pre-commit/pre-merge checks for the repository", (RepositoryChecker checker, AltinnRepository repository) =>
{
    return CheckCommandResult.Full(checker, repository);
});

cli.AddCommand("github", "GitHub operations", (builder) =>
{
    builder.AddCommand("upload", "Upload artifacts to a GitHub release", (
        GitHubContext context,
        [Argument(Description = "The release ID.")] long releaseId,
        [Argument(Description = "File glob to upload.")] string glob,
        GitHubService github)
        => github.UploadArtifactsToRelease(context, releaseId, glob));
});

cli.AddCommand("nuget", "NuGet operations", (builder) =>
{
    builder.AddCommand("publish", "Publish NuGet packages to a NuGet feed", (
        [Argument(Description = "File glob to publish.")] string glob,
        [RestArguments] string[] args,
        NuGetService nuget)
        => nuget.PublishPackages(glob, args));
});

cli.AddCommand("ci", "CI Helpers", (builder) =>
{
    builder.AddCommand<GetPathFiltersCommand>("get-path-filters", "Get named path-filters for verticals in the repository");
    builder.AddCommand<FindVerticalsCommand>("find-verticals", "Find verticals in the repository that should be ran CI for");
});

return await cli.RunAsync(args);
