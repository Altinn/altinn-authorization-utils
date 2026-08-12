using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Formatting;
using Altinn.Authorization.RepoCtl.Binding;
using Altinn.Authorization.RepoCtl.Checks;
using Altinn.Authorization.RepoCtl.Formatting;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Options;
using Altinn.Authorization.RepoCtl.Solutions;
using Microsoft.Extensions.DependencyInjection;

var builder = CliApplication.CreateBuilder("Altinn Authorization Repository Manager (repoctl)");
builder.Services.AddSingleton<AltinnRepositoryResolver>();
builder.Services.AddSingleton<IConfigureOption, ConfigureAltinnVerticalKindOptions>();
builder.Services.AddSingleton<ICommandHandlerParameterBinderResolver, AltinnRepositoryBinderResolver>();
builder.Services.AddSingleton<AltinnRepositoryLoader>();
builder.Services.AddSingleton<SolutionService>();
builder.Services.AddSingleton<Checker>();
builder.Services.AddSingleton<RepositoryChecker>();
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

return await cli.RunAsync(args);
